using Diagnosis.Core;
using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Data;
using EventAggregator;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class AppointmentViewModel : ViewModelBase
    {
        #region Fileds

        internal readonly Appointment appointment;
        internal Func<HealthRecordViewModel> OpenedHrGetter;
        internal Action<HealthRecordViewModel> OpenedHrSetter;
        private HealthRecordManager hrManager;

        private static HrEditorViewModel _hrEditorStatic;//= new HrEditorViewModel();

        private ICollectionView healthRecordsView;

        #endregion Fileds

        #region Model

        public Doctor Doctor
        {
            get { return appointment.Doctor; }
        }

        public DateTime DateTime
        {
            get
            {
                return appointment.DateAndTime;
            }
        }

        public ObservableCollection<HealthRecordViewModel> HealthRecords
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

        #endregion Model

        public HrEditorViewModel HealthRecordEditor { get { return _hrEditorStatic; } }

        public HealthRecordViewModel SelectedHealthRecord
        {
            get
            {
                return OpenedHrGetter != null ? OpenedHrGetter() : null;
            }
            set
            {
                OpenedHrSetter(value);
            }
        }

        #region Commands

        public ICommand AddHealthRecordCommand
        {
            get
            {
                return new RelayCommand(() =>
                    {
                        AddHealthRecord();
                    },
                    // нельзя добавлять новую запись, пока выбранная пуста
                    () => SelectedHealthRecord == null || !SelectedHealthRecord.IsEmpty
                    );
            }
        }

        public ICommand DeleteHealthRecordsCommand
        {
            get
            {
                return new RelayCommand(() =>
                    {
                        DeleteCheckedHealthRecords();
                    }, () => CheckedHealthRecords > 0);
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
                        }, () => CheckedHealthRecords > 0);
            }
        }

        public ICommand EditHrCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            HealthRecordEditor.HealthRecord.Editable.ToggleEditor();
                        }, () => HealthRecordEditor.HealthRecord != null);
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

        public int CheckedHealthRecords
        {
            get
            {
                return HealthRecords.Where(hr => hr.IsChecked).Count();
            }
        }

        public AppointmentViewModel(Appointment appointment)
        {
            Contract.Requires(appointment != null);
            this.appointment = appointment;

            appointment.HealthRecords.CollectionChanged += HealthRecords_CollectionChanged;

            hrManager = new HealthRecordManager(this);
            hrManager.HrPropertyChanged += hrManager_HrPropertyChanged;
        }


        public void AddHealthRecord()
        {
            hrManager.AddHealthRecord();
        }

        /// <summary>
        /// Вызывается при смене открытой записи.
        /// </summary>
        internal void OnOpenedHealthRecordChanged()
        {
            HealthRecordEditor.HealthRecord = SelectedHealthRecord;
            //if (!DebugOutput.test)
            OnPropertyChanged("SelectedHealthRecord");
        }

        void HealthRecords_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    if (SelectedHealthRecord.healthRecord == item)
                        MoveHrViewSelection();
                }
            }
            OnPropertyChanged("IsEmpty");
        }

        private void hrManager_HrPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Category")
            {
                healthRecordsView.Refresh();
            }
            else if (e.PropertyName == "IsChecked")
            {
                OnPropertyChanged("CheckedHealthRecords");
            }
        }

        private void MoveHrViewSelection()
        {
            // удалена выделенная запись - меняем выделение
            var i = healthRecordsView.CurrentPosition;
            if (i == HealthRecords.Count - 1)
            {
                // удалили последную запись в списке
                healthRecordsView.MoveCurrentToPrevious();
            }
            else
            {
                healthRecordsView.MoveCurrentToNext();
            }
        }

        private void DeleteCheckedHealthRecords()
        {
            HealthRecords.Where(hr => hr.IsChecked).ToList().ForAll(hr =>
            {
                hr.Editable.Delete();
                // uncheck after delete, to SelectedHealthRecord be deletable
                hr.IsChecked = false;
            });
        }

        public override string ToString()
        {
            return appointment.ToString();
        }
    }
}