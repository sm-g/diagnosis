using Diagnosis.Common;
using Diagnosis.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Data;
using GongSolutions.Wpf.DragDrop;
using System.Windows.Input;
using System.Windows;
using System;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.Common.Util;

namespace Diagnosis.ViewModels.Screens
{
    public enum HrViewColumn
    {
        None,
        [LocalizableDescription(@"Sorting_Category")]
        Category,
        [LocalizableDescription(@"Sorting_Ord")]
        Ord,
        [LocalizableDescription(@"Sorting_Date")]
        Date
    }

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
        private bool _rectSelect;
        private HrViewColumn _sort;
        private HrViewColumn _group;
        private bool _dragSource;
        private bool _focused;

        public event EventHandler<ListEventArgs<HealthRecord>> SaveNeeded;

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
                    var sel = SelectedHealthRecords;
                    healthRecordsView.Refresh();
                    sel.ForAll(vm => vm.IsSelected = true); // fix only one selected after Refresh
                }
                else if (e.PropertyName == "IsChecked")
                {
                    OnPropertyChanged(() => CheckedHrCount);
                }
            });
            hrManager.Undeleted += (s, e) =>
            {
                OnSaveNeeded(new List<HealthRecord>() { e.entity as HealthRecord });
            };

            HealthRecords.CollectionChanged += (s, e) =>
            {
                HolderVm.UpdateIsEmpty();
            };

            healthRecordsView = (CollectionView)CollectionViewSource.GetDefaultView(HealthRecords);
            Grouping = HrViewColumn.Category;
            Sorting = HrViewColumn.None;

            DropHandler = new DropTargetHandler(this);
            DragHandler = new DragSourceHandler();

            IsRectSelectEnabled = true;
            IsDragSourceEnabled = true;

            SelectHealthRecord(hrViewer.GetLastSelectedFor(holder));
        }

        public HolderViewModel HolderVm { get; private set; }

        public ObservableCollection<ShortHealthRecordViewModel> HealthRecords { get { return hrManager.HealthRecords; } }

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

        public IEnumerable<ShortHealthRecordViewModel> SelectedHealthRecords
        {
            get { return HealthRecords.Where(vm => vm.IsSelected).ToList(); }
        }

        public bool AddingHrByCommnd { get; private set; }
        #region Commands

        public ICommand AddHealthRecordCommand
        {
            get
            {
                return new RelayCommand(() =>
                    {

                        var lastHrVM = SelectedHealthRecord ?? HealthRecords.LastOrDefault();
                        var newHr = AddHr(true);

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
                        var toSave = hrManager.GetSelectedHrs();
                        hrManager.DeleteCheckedHealthRecords();
                        OnSaveNeeded(toSave);
                    }, () => CheckedHrCount > 0);
            }
        }

        public ICommand SendToSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            this.Send(Event.SendToSearch, hrManager.GetSelectedHrs()
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
        public DropTargetHandler DropHandler { get; private set; }

        public DragSourceHandler DragHandler { get; private set; }

        public bool IsRectSelectEnabled
        {
            get
            {
                return _rectSelect;
            }
            set
            {
                if (_rectSelect != value)
                {
                    _rectSelect = value;
                    OnPropertyChanged(() => IsRectSelectEnabled);
                }
            }
        }


        public bool IsDragSourceEnabled
        {
            get
            {
                return _dragSource;
            }
            set
            {
                if (_dragSource != value)
                {
                    _dragSource = value;
                    OnPropertyChanged(() => IsDragSourceEnabled);
                }
            }
        }

        public bool IsFocused
        {
            get
            {
                return _focused;
            }
            set
            {
                if (_focused != value)
                {
                    _focused = value;
                    //logger.DebugFormat("HrList focused {0} ", value);

                    OnPropertyChanged(() => IsFocused);
                }
            }
        }

        public HrViewColumn Sorting
        {
            get
            {
                return _sort;
            }
            set
            {
                if (_sort != value)
                {
                    _sort = value;
                    healthRecordsView.SortDescriptions.Clear();
                    if (value != HrViewColumn.None)
                    {
                        var sort = new SortDescription(value.ToString(), ListSortDirection.Ascending);
                        healthRecordsView.SortDescriptions.Add(sort);
                    }
                    OnPropertyChanged(() => Sorting);
                }
            }
        }

        public HrViewColumn Grouping
        {
            get
            {
                return _group;
            }
            set
            {
                if (_group != value)
                {
                    _group = value;
                    healthRecordsView.GroupDescriptions.Clear();
                    if (value != HrViewColumn.None)
                    {
                        var grDesc = new PropertyGroupDescription(value.ToString());
                        healthRecordsView.GroupDescriptions.Add(grDesc);
                    }
                    OnPropertyChanged(() => Grouping);
                }
            }
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
                    var newHR = AddHr();
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

                    var newHR = AddHr();
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

        internal void RemoveDeleted()
        {
            hrManager.MakeDeletions();
        }

        private HealthRecord AddHr(bool fromCommand = false)
        {
            AddingHrByCommnd = fromCommand;
            var newHr = holder.AddHealthRecord(AuthorityController.CurrentDoctor);
            AddingHrByCommnd = false;
            return newHr;
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

        public class DropTargetHandler : DefaultDropHandler
        {
            private readonly HrListViewModel master;

            public DropTargetHandler(HrListViewModel master)
            {
                this.master = master;
            }

            public bool FromSameHrList(IDropInfo dropInfo)
            {
                var sourceList = GetList(dropInfo.DragInfo.SourceCollection);
                return sourceList == master.HealthRecords;
            }

            public bool FromAutocomplete(IDropInfo dropInfo)
            {
                var sourceList = GetList(dropInfo.DragInfo.SourceCollection);
                return sourceList is IEnumerable<TagViewModel>;
            }

            public override void DragOver(IDropInfo dropInfo)
            {
                var data = ExtractData(dropInfo.Data).Cast<object>();
                if (dropInfo.DragInfo == null || dropInfo.DragInfo.SourceCollection == null || data.Count() == 0)
                {
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }
                if (FromSameHrList(dropInfo))
                {
                    dropInfo.Effects = DragDropEffects.Move;
                }
                else if (FromAutocomplete(dropInfo))
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                }
                else
                {
                    dropInfo.Effects = DragDropEffects.Scroll;
                }
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            }

            public override void Drop(IDropInfo dropInfo)
            {
                var data = ExtractData(dropInfo.Data).Cast<object>();

                logger.DebugFormat("ddrop {0} {1}", data.Count(), data.First().GetType());

                var insertIndex = dropInfo.InsertIndex;

                if (FromSameHrList(dropInfo))
                {
                    // drop hrs from hrslist
                    var hrs = data.Cast<ShortHealthRecordViewModel>();
                    logger.DebugFormat("selected bef {0} ", master.SelectedHealthRecords.Count());

                    // reorder hrs
                    foreach (var hr in hrs)
                    {
                        var old = master.HealthRecords.IndexOf(hr);
                        var n = old < insertIndex ? insertIndex - 1 : insertIndex;

                        logger.DebugFormat("move {0} - {1}", old, n);
                        if (old != n)
                        {
                            master.HealthRecords.Move(old, n);

                            // меняем порядок

                            //var down = old < insertIndex;
                            //for (int i = down ? old+1 : n; i < (down ? n+1 : old); i ++)
                            //{
                            //    master.HealthRecords[i].healthRecord.Ord += (down ? -1 : 1);

                            //}
                            //master.HealthRecords[old].healthRecord.Ord = n;

                            // remove insert

                            //var t1 = master.HealthRecords[old];
                            //master.HealthRecords.RemoveAt(old);
                            //master.HealthRecords.Insert(n, t1);
                        }
                        if (dropInfo.TargetGroup != null)
                            hr.healthRecord.Category = dropInfo.TargetGroup.Name as HrCategory;
                    }
                    logger.DebugFormat("selected after dd {0} ", master.SelectedHealthRecords.Count());

                }
                else if (FromAutocomplete(dropInfo))
                {
                    // drop tags from autocomplete

                    var tags = data.Cast<TagViewModel>();

                    // new hr from tags
                    var newHR = master.AddHr();
                    var items = tags.Select(t => t.Entity).ToList();
                    newHR.SetItems(items);
                }
                //if (dropInfo.TargetCollection is ICollectionView)
                //{
                //    ((ICollectionView)dropInfo.TargetCollection).Refresh();
                //}
                master.OnSaveNeeded();
                logger.DebugFormat("selected after save {0} ", master.SelectedHealthRecords.Count());


            }
        }

        public class DragSourceHandler : IDragSource
        {
            /// <summary>
            /// Data is hrs vm.
            /// </summary>
            /// <param name="dragInfo"></param>
            public void StartDrag(IDragInfo dragInfo)
            {
                var hrs = dragInfo.SourceItems.Cast<ShortHealthRecordViewModel>();
                var itemCount = hrs.Count();

                if (itemCount == 1)
                {
                    dragInfo.Data = hrs.First();

                }
                else if (itemCount > 1)
                {
                    dragInfo.Data = GongSolutions.Wpf.DragDrop.Utilities.TypeUtilities.CreateDynamicallyTypedList(hrs);
                }
                dragInfo.Effects = (dragInfo.Data != null) ?
                                     DragDropEffects.Copy | DragDropEffects.Move :
                                     DragDropEffects.None;
            }

            public bool CanStartDrag(IDragInfo dragInfo)
            {
                return dragInfo.SourceItems.Cast<ShortHealthRecordViewModel>().Count() > 0;
            }

            public void DragCancelled()
            {
            }

            public void Dropped(IDropInfo dropInfo)
            {
            }
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
            public HealthRecordUnit Unit { get; set; }
            public List<IHrItemObject> Hios { get; set; }


        }

    }
}