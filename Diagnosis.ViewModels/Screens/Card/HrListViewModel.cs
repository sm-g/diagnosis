using Diagnosis.Core;
using Diagnosis.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class HrListViewModel : ViewModelBase
    {
        private static HrViewer viewer = new HrViewer();
        internal readonly IHrsHolder holder;
        private HealthRecordManager hrManager;
        private ICollectionView healthRecordsView;
        private ShortHealthRecordViewModel _selectedHealthRecord;

        public ObservableCollection<ShortHealthRecordViewModel> HealthRecords
        {
            get
            {
                if (healthRecordsView == null)
                {
                    healthRecordsView = (CollectionView)CollectionViewSource.GetDefaultView(hrManager.HealthRecords);
                    PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
                    SortDescription sort1 = new SortDescription("Category", ListSortDirection.Ascending);
                    SortDescription sort2 = new SortDescription("SortingDate", ListSortDirection.Ascending);
                    healthRecordsView.GroupDescriptions.Add(groupDescription);
                    healthRecordsView.SortDescriptions.Add(sort1);
                    healthRecordsView.SortDescriptions.Add(sort2);
                }
                return hrManager.HealthRecords;
            }
        }

        public ShortHealthRecordViewModel SelectedHealthRecord
        {
            get
            {
                return _selectedHealthRecord;
            }
            set
            {
                if (_selectedHealthRecord == value)
                    return;

                if (value != null)
                    viewer.Select(value.healthRecord, holder);

                _selectedHealthRecord = value;
                OnPropertyChanged(() => SelectedHealthRecord);
            }
        }

        #region Commands

        public ICommand AddHealthRecordCommand
        {
            get
            {
                return new RelayCommand(() =>
                    {
                        var lastHrVM = SelectedHealthRecord ?? HealthRecords.LastOrDefault();
                        var newHr = holder.AddHealthRecord();
                        if (lastHrVM != null)
                        {
                            // копируем категории из последней записи
                            newHr.Category = lastHrVM.healthRecord.Category;
                        }
                    },
                    // нельзя добавлять новую запись, пока выбранная пуста
                    () => SelectedHealthRecord == null || !SelectedHealthRecord.healthRecord.IsEmpty()
                    );
            }
        }

        public ICommand DeleteHealthRecordsCommand
        {
            get
            {
                return new RelayCommand(() =>
                    {
                        hrManager.DeleteCheckedHealthRecords();
                    }, () => CheckedHrCount > 0);
            }
        }

        public ICommand SendHealthRecordsToSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            this.Send(Events.SendToSearch, HealthRecords.Where(hr => hr.IsChecked)
                                .Select(vm => vm.healthRecord)
                                .AsParams(MessageKeys.HealthRecords));
                        }, () => CheckedHrCount > 0);
            }
        }

        public RelayCommand<bool> MoveHrSelectionCommand
        {
            get
            {
                return new RelayCommand<bool>((up) =>
                        {
                            if (up)
                            {
                                if (healthRecordsView.CurrentPosition != 0)
                                    healthRecordsView.MoveCurrentToPrevious();
                                else
                                    healthRecordsView.MoveCurrentToLast();
                            }
                            else
                            {
                                if (healthRecordsView.CurrentPosition != HealthRecords.Count - 1)
                                    healthRecordsView.MoveCurrentToNext();
                                else
                                    healthRecordsView.MoveCurrentToFirst();
                            }
                        });
            }
        }

        #endregion Commands

        public int CheckedHrCount
        {
            get
            {
                return HealthRecords.Where(hr => hr.IsChecked).Count();
            }
        }

        public HrListViewModel(IHrsHolder holder)
        {
            Contract.Requires(holder != null);
            this.holder = holder;

            hrManager = new HealthRecordManager(holder, onHrVmPropChanged: (s, e) =>
            {
                if (e.PropertyName == "Category")
                {
                    healthRecordsView.Refresh();
                }
                else if (e.PropertyName == "IsChecked")
                {
                    OnPropertyChanged("CheckedHrCount");
                }
            });

            SelectHealthRecord(viewer.GetLastSelectedFor(holder));
        }

        public override string ToString()
        {
            return holder.ToString();
        }

        internal void SelectHealthRecord(HealthRecord healthRecord)
        {
            SelectedHealthRecord = HealthRecords.FirstOrDefault(x => x.healthRecord == healthRecord);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    hrManager.Dispose();
                    MakeDeletions();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Реальное удаление удаленных и пустых записей.
        /// </summary>
        private void MakeDeletions()
        {
            this.Send(Events.HideOverlay, typeof(HealthRecord).AsParams(MessageKeys.Type));

            foreach (var hrVm in hrManager.DeletedHealthRecords)
            {
                holder.RemoveHealthRecord(hrVm.healthRecord);
            }
            hrManager.DeletedHealthRecords.Clear();

            var emptyHrs = holder.HealthRecords.Where(hr => hr.IsEmpty()).ToList();
            emptyHrs.ForEach(hr => holder.RemoveHealthRecord(hr));
        }
    }
}