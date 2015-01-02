using Diagnosis.Common;
using Diagnosis.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows;
using System;
using Diagnosis.ViewModels.Search.Autocomplete;

namespace Diagnosis.ViewModels.Screens
{
    public class HrListViewModel : ViewModelBase, IClipboardTarget
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HrListViewModel));
        private static HrViewer hrViewer = new HrViewer();
        internal readonly IHrsHolder holder;
        private HealthRecordManager hrManager;
        private ICollectionView healthRecordsView;
        private ShortHealthRecordViewModel _selectedHealthRecord;
        private Action<HealthRecord, HrData.HrInfo> fillHr;
        private Action<List<IHrItemObject>> syncHios;
        private bool inSelectMany;

        public event EventHandler<ListEventArgs<HealthRecord>> SaveNeeded;

        public HolderViewModel HolderVm { get; private set; }

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
                    SortDescription sort3 = new SortDescription("Ord", ListSortDirection.Ascending);
                    healthRecordsView.GroupDescriptions.Add(groupDescription);
                    healthRecordsView.SortDescriptions.Add(sort3);
                    //healthRecordsView.SortDescriptions.Add(sort2);
                }
                return hrManager.HealthRecords;
            }
        }

        public ShortHealthRecordViewModel SelectedHealthRecord
        {
            get { return _selectedHealthRecord; }
            set
            {
                if (_selectedHealthRecord == value)
                    return;

                if (_selectedHealthRecord != null && !inSelectMany) // снимаем выделение с прошлой выделенной
                {
                    _selectedHealthRecord.IsSelected = false;
                }
                if (value != null)
                {
                    hrViewer.Select(value.healthRecord, holder);
                    value.IsSelected = true;
                }
                logger.DebugFormat("hrList selected {0} -> {1}", _selectedHealthRecord, value);
                _selectedHealthRecord = value;
                OnPropertyChanged(() => SelectedHealthRecord);
            }
        }


        public bool InAddHrCommand { get; private set; }
        #region Commands

        public ICommand AddHealthRecordCommand
        {
            get
            {
                return new RelayCommand(() =>
                    {

                        var lastHrVM = SelectedHealthRecord ?? HealthRecords.LastOrDefault();
                        InAddHrCommand = true;
                        var newHr = holder.AddHealthRecord(AuthorityController.CurrentDoctor);
                        InAddHrCommand = false;

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

        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                    {
                        hrManager.DeleteCheckedHealthRecords();
                    }, () => CheckedHrCount > 0);
            }
        }

        public ICommand SendToSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            this.Send(Events.SendToSearch, hrManager.GetSelectedHrs()
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

        public bool ShowCanAlso
        {
            get
            {
                return !(holder is Appointment && !holder.IsEmpty()); // непустой осмотр - ничего дополнительного
            }
        }

        public HrListViewModel(IHrsHolder holder, Action<HealthRecord, HrData.HrInfo> filler, Action<List<IHrItemObject>> syncer)
        {
            Contract.Requires(holder != null);
            Contract.Requires(filler != null);
            Contract.Requires(syncer != null);
            this.holder = holder;
            this.fillHr = filler;
            this.syncHios = syncer;

            HolderVm = new HolderViewModel(holder);

            hrManager = new HealthRecordManager(holder, onHrVmPropChanged: (s, e) =>
            {
                if (e.PropertyName == "Category")
                {
                    healthRecordsView.Refresh();
                }
                else if (e.PropertyName == "IsChecked")
                {
                    OnPropertyChanged(() => CheckedHrCount);
                }
            });

            HealthRecords.CollectionChanged += (s, e) =>
            {
                HolderVm.UpdateIsEmpty();
            };

            SelectHealthRecord(hrViewer.GetLastSelectedFor(holder));
        }
        public void Cut()
        {
            logger.Debug("cut");
            Copy();
            hrManager.DeleteCheckedHealthRecords(withCancel: false);
        }

        public void Copy()
        {
            var hrs = hrManager.GetSelectedHrs();
            var hrInfos = hrs.Select(hr => new HrData.HrInfo()
            {
                HolderId = (Guid)hr.Holder.Id,
                DoctorId = hr.Doctor.Id,
                CategoryId = hr.Category != null ? (int?)hr.Category.Id : null,
                FromDay = hr.FromDay,
                FromMonth = hr.FromMonth,
                FromYear = hr.FromYear,
                Unit = hr.Unit,
                Hios = new List<IHrItemObject>(hr.HrItems.Select(x => x.Entity))

            }).ToList();

            var data = new HrData() { Hrs = hrInfos };

            var strings = string.Join(".\n", hrs.Select(hr => string.Join(", ", hr.GetOrderedEntities()))) + ".";

            IDataObject dataObj = new DataObject(HrData.DataFormat.Name, data);
            dataObj.SetData(System.Windows.DataFormats.UnicodeText, strings);
            Clipboard.SetDataObject(dataObj, false);

            LogHrs("copy", hrInfos);
        }


        public void Paste()
        {
            TagData data = null;
            var ido = Clipboard.GetDataObject();

            if (ido.GetDataPresent(TagData.DataFormat.Name))
            {
                data = (TagData)ido.GetData(TagData.DataFormat.Name);
            }
            if (data != null)
            {
                syncHios(data.ItemObjects);

                var hrs = hrManager.GetSelectedHrs();
                if (hrs.Count > 0)
                {
                    // add hios to end of Selected Hrs
                    hrs.ForAll(hr => hr.AddItems(data.ItemObjects));
                    OnSaveNeeded(hrManager.GetSelectedHrs());

                }
                else
                {
                    // new hr with pasted hios
                    var newHR = holder.AddHealthRecord(AuthorityController.CurrentDoctor);
                    newHR.AddItems(data.ItemObjects);
                    OnSaveNeeded(); // save all
                }
                LogHrItemObjects("paste", data.ItemObjects);
            }


            HrData hrDat = null;

            if (ido.GetDataPresent(HrData.DataFormat.Name))
            {
                hrDat = (HrData)ido.GetData(HrData.DataFormat.Name);
            }
            if (hrDat != null)
            {
                // paste hrs TODO before first Selected
                var index = HealthRecords.IndexOf(SelectedHealthRecord);

                HealthRecords.ForAll(vm => vm.IsSelected = false);

                var pasted = new List<HealthRecord>();
                foreach (var hr2 in hrDat.Hrs)
                {
                    if (hr2 == null) continue;

                    var newHR = holder.AddHealthRecord(AuthorityController.CurrentDoctor);
                    fillHr(newHR, hr2);
                    pasted.Add(newHR);
                }
                OnSaveNeeded(); // save all

                SelectHealthRecords(pasted);
                LogHrs("paste", hrDat.Hrs);
            }

        }
        public override string ToString()
        {
            return "HrList for " + holder.ToString();
        }

        internal void SelectHealthRecord(HealthRecord healthRecord)
        {
            SelectedHealthRecord = HealthRecords.FirstOrDefault(x => x.healthRecord == healthRecord);
        }
        internal void SelectHealthRecords(IEnumerable<HealthRecord> hrs)
        {
            HealthRecords.Where(vm => hrs.Contains(vm.healthRecord))
                .ForAll(vm => vm.IsSelected = true);

            inSelectMany = true;
            SelectHealthRecord(hrs.LastOrDefault());
            inSelectMany = false;
        }

        private void LogHrs(string action, IEnumerable<HrData.HrInfo> hrs)
        {
            logger.DebugFormat("{0} hrs with hios: {1}", action, string.Join("\n", hrs.Select((hr, i) => string.Format("{0} {1}", i, hr.Hios.FlattenString()))));
        }
        private void LogHrItemObjects(string action, IEnumerable<IHrItemObject> hios)
        {
            logger.DebugFormat("{0} hios: {1}", action, hios.FlattenString());
        }

        protected virtual void OnSaveNeeded(List<HealthRecord> hrsToSave = null)
        {
            var h = SaveNeeded;
            if (h != null)
            {
                h(this, new ListEventArgs<HealthRecord>(hrsToSave));
            }
        }
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    hrManager.Dispose();
                    HolderVm.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

    }

    [Serializable]
    public class ListEventArgs<T> : EventArgs
    {
        public readonly IList<T> list;

        [System.Diagnostics.DebuggerStepThrough]
        public ListEventArgs(IList<T> list)
        {
            this.list = list;
        }
    }

    [Serializable]
    public class HrData
    {
        public static readonly DataFormat DataFormat = DataFormats.GetDataFormat("hr");
        public List<HrInfo> Hrs { get; set; }
        [Serializable]

        public class HrInfo
        {
            public Guid HolderId { get; set; }
            public Guid DoctorId { get; set; }
            public int? CategoryId { get; set; }
            public int? FromDay { get; set; }
            public int? FromMonth { get; set; }
            public int? FromYear { get; set; }
            public HealthRecordUnits Unit { get; set; }
            public List<IHrItemObject> Hios { get; set; }


        }

    }
}