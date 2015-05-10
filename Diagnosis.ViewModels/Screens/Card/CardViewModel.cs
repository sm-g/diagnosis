using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
using EventAggregator;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public partial class CardViewModel : ScreenBaseViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CardViewModel));
        private static PatientViewer viewer; // static to hold history
        private HrListViewModel _hrList;
        private HeaderViewModel _header;
        private readonly HrEditorViewModel _hrEditor;
        private readonly NavigatorViewModel _navigator;

        private bool editorWasOpened;
        private Saver saver;
        private EventMessageHandlersManager handlers;
        private Doctor doctor;

        public CardViewModel(bool resetHistory = false)
        {
            if (IsInDesignMode) return;
            if (resetHistory || viewer == null)
                ResetHistory();

            doctor = AuthorityController.CurrentDoctor;

            saver = new Saver(Session);
            _navigator = new NavigatorViewModel(viewer);
            _hrEditor = new HrEditorViewModel(Session);

            Navigator.CurrentChanged += (s, e) =>
            {
                // add to history
                HrEditor.Unload(); // закрываем редактор при смене активной сущности

                var holder = e.holder;

                ShowHrsList(holder);
                ShowHeader(holder);
            };
            Navigator.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CurrentTitle")
                {
                    Title = Navigator.CurrentTitle;
                }
            };

            HrEditor.Unloaded += (s, e) =>
            {
                // сохраняем запись
                var hr = e.hr as HealthRecord;
                if (hr.Doctor == doctor)  // добавлять только если врач редактировал свою запись?
                    doctor.AddWords(hr.Words);
                saver.Save(hr);
            };
            HrEditor.Closing += (s, e) =>
            {
                // закрыт по команде — переходим к списку записей
                // logger.DebugFormat("hreditor closed, listfocused = {0}", HrList.IsFocused);
                HrList.IsFocused = true;
                // restore selected?
            };

            HrEditor.Closed += (s, e) =>
            {
            };

            handlers = new EventMessageHandlersManager(new EventMessageHandler[] {
                this.Subscribe(Event.DeleteHolder, (e) =>
                {
                    var holder = e.GetValue<IHrsHolder>(MessageKeys.Holder);

                    // убрать из результатов поиска (или проверять при открытии, удален ли)
                    viewer.RemoveFromHistory(holder);

                    if (holder is Patient)
                    {
                        saver.SaveWithCleanup(viewer.OpenedPatient);

                        Navigator.Remove(holder as Patient);
                        saver.Delete(holder);
                        if (Navigator.TopCardItems.Count == 0)
                            OnLastItemRemoved();
                        return;
                    }
                    else if (holder is Course)
                    {
                        var course = holder as Course;
                        course.Patient.RemoveCourse(course);
                        saver.SaveWithCleanup(viewer.OpenedPatient); // сохраняем на случай, если удаление при открытом пациенте — список записей не меняется
                    }
                    else if (holder is Appointment)
                    {
                        var app = holder as Appointment;
                        app.Course.RemoveAppointment(app);
                        saver.SaveWithCleanup(viewer.OpenedPatient);
                    }
                }),
                this.Subscribe(Event.AddHr, (e) =>
                    {
                    var h= e.GetValue<IHrsHolder>(MessageKeys.Holder);
                    var startEdit= e.GetValue<bool>(MessageKeys.Boolean);

                    AddHr(h, startEdit);
                    })
                }
            );
        }

        /// <summary>
        /// Создает карту и тут же вызывает Open(entity).
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="resetHistory"></param>
        public CardViewModel(object entity, bool resetHistory = false)
            : this(resetHistory)
        {
            Open(entity);
        }

        /// <summary>
        /// После удаления всех элементов, карточка пуста.
        /// </summary>
        public event EventHandler LastItemRemoved;

        public HrListViewModel HrList
        {
            get
            {
                return _hrList;
            }
            set
            {
                if (_hrList != value)
                {
                    _hrList = value;
                    OnPropertyChanged(() => HrList);
                }
            }
        }

        public HrEditorViewModel HrEditor { get { return _hrEditor; } }

        public NavigatorViewModel Navigator { get { return _navigator; } }

        public HeaderViewModel Header
        {
            get
            {
                return _header;
            }
            set
            {
                if (_header != value)
                {
                    if (_header != null)
                        _header.Dispose();
                    _header = value;
                    OnPropertyChanged(() => Header);
                }
            }
        }

        public RelayCommand StartCourseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    viewer.OpenedPatient.AddCourse(AuthorityController.CurrentDoctor);
                });
            }
        }

        public RelayCommand AddAppointmentCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    viewer.OpenedCourse.AddAppointment(AuthorityController.CurrentDoctor);
                },
                () => viewer.OpenedCourse != null && viewer.OpenedCourse.End == null);
            }
        }

        public RelayCommand ToggleEditorCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ToogleHrEditor();
                }, () => HrList.SelectedHealthRecord != null);
            }
        }

        /// <summary>
        /// Открывает/закрывает редактор для последней выбранной в списке записи.
        /// </summary>
        public void ToogleHrEditor()
        {
            Contract.Ensures(HrList.LastSelected == null ||
                HrEditor.HasHealthRecord != Contract.OldValue(HrEditor.HasHealthRecord));

            if (HrList.LastSelected == null)
                return;

            // logger.DebugFormat("toggle hr editor from {0}", HrEditor.HasHealthRecord);
            if (HrEditor.HasHealthRecord)
            {
                // TODO Contract.Assume(HrEditor.HealthRecord.healthRecord == HrList.LastSelected.healthRecord);
                HrEditor.Unload();
            }
            else
            {
                HrEditor.Load(HrList.LastSelected.healthRecord);
            }
        }

        /// <summary>
        /// Открывает запись и редактор для записи и начинает редактирование слов.
        /// </summary>
        public void FocusHrEditor(HealthRecord hr, bool addToSelected = true)
        {
            Contract.Requires(hr != null);
            Contract.Assume(hr.Holder == HrList.holder);

            //logger.DebugFormat("FocusHrEditor to {0}", hr);
            HrList.SelectHealthRecord(hr, addToSelected);
            HrEditor.Load(hr);
            HrEditor.Autocomplete.StartEdit();
        }

        public void ResetHistory()
        {
            viewer = new PatientViewer();
            HrListViewModel.ResetHistory();
        }

        /// <summary>
        /// Открывает держателя или выделяет записи.
        /// </summary>
        internal void Open(object toOpen, bool lastAppOrCourse = false)
        {
            Contract.Requires(toOpen != null);

            logger.DebugFormat("open {0}", toOpen);

            if (toOpen is IHrsHolder)
                OpenHolder(toOpen as IHrsHolder, lastAppOrCourse);
            else if (toOpen is HealthRecord)
                OpenHr(toOpen as HealthRecord);
            else if (toOpen is IEnumerable<HealthRecord>)
                OpenHrs(toOpen as IEnumerable<HealthRecord>);
            else throw new ArgumentException("toOpen");
        }

        private void OpenHolder(IHrsHolder holder, bool lastAppOrCourse)
        {
            Contract.Ensures(HrList.holder == holder.Actual || lastAppOrCourse);

            holder = holder.Actual as IHrsHolder;
            var was = viewer.AutoOpen;
            if (lastAppOrCourse)
                viewer.AutoOpen = true;

            Navigator.NavigateTo(holder);

            viewer.AutoOpen = was;
        }

        private void OpenHr(HealthRecord hr)
        {
            Contract.Ensures(HrList.holder == hr.Holder.Actual);

            OpenHolder(hr.Holder, false);
            HrList.SelectHealthRecord(hr);
        }

        private void OpenHrs(IEnumerable<HealthRecord> hrs)
        {
            Contract.Ensures(!hrs.Any() || HrList.holder == hrs.First().Holder);

            var first = hrs.FirstOrDefault();
            if (first != null)
            {
                OpenHolder(first.Holder, false);
                // если есть удаленные записи - просто не будут выделены
                HrList.SelectHealthRecords(hrs);
            }
        }

        protected virtual void OnLastItemRemoved()
        {
            var h = LastItemRemoved;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    HrEditor.Dispose();

                    CloseHeader();
                    CloseHrList();

                    viewer.CloseAll();

                    Navigator.Dispose();

                    handlers.Dispose();

                    saver.Save(doctor);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Сохраняет записи в списке, все или переданные.
        /// </summary>
        private void SaveHealthRecords(object s, ListEventArgs<HealthRecord> e)
        {
            logger.DebugFormat("SaveNeeded for hrs: {0}", e.list == null ? "All" : e.list.Count.ToString());

            if (e.list == null)
            {
                SaveAllHrs();
            }
            else
            {
                // вставка/дроп тегов в записи
                // изменение видимости (IsDeleted)

                saver.Save(e.list.ToArray());
                if (!HrEditor.HasHealthRecord)
                {
                    HrList.IsFocused = true;
                }
            }
        }

        /// <summary>
        /// Cохраняем все записи кроме открытой в редакторе
        /// </summary>
        internal void SaveAllHrs()
        {
            // новые записи — вставка/дроп записей/тегов на список
            // смена порядка — дроп записей

            Contract.Assume(HrList.HealthRecords.IsStrongOrdered(x => x.Ord));

            saver.Save(HrList.HealthRecords
                .Select(vm => vm.healthRecord)
                .Except(HrEditor.HasHealthRecord ? HrEditor.HealthRecord.healthRecord.ToEnumerable() : Enumerable.Empty<HealthRecord>())
                .ToArray());
        }

        /// <summary>
        /// Показвает записи активной сущности.
        /// </summary>
        /// <param name="holder"></param>
        private void ShowHrsList(IHrsHolder holder)
        {
            if (HrList != null)
            {
                if (HrList.holder == holder)
                    return; // список может быть уже создан

                CloseHrList();
            }

            if (holder != null)
            {
                HrList = new HrListViewModel(holder, (hr, hrInfo) =>
                {
                    // заполняем после вставки записи

                    hrInfo.Chios.SyncAfterPaste(Session);

                    if (hrInfo.CategoryId != null)
                    {
                        using (var tr = Session.BeginTransaction())
                        {
                            hr.Category = Session.Get<HrCategory>(hrInfo.CategoryId.Value);
                        }
                    }
                    hr.FromDate.FillDateAndNowFrom(hrInfo.From);
                    hr.ToDate.FillDateAndNowFrom(hrInfo.To);

                    var unit = hrInfo.Unit;
                    // если вставляем к пациенту без возраста
                    if (hr.GetPatient().BirthYear == null && hrInfo.Unit == HealthRecordUnit.ByAge)
                        unit = HealthRecordUnit.NotSet;

                    hr.Unit = unit;
                    hr.SetItems(hrInfo.Chios);
                }, (hios) =>
                {
                    hios.SyncAfterPaste(Session);
                });

                HrViewColumn gr;
                if (Enum.TryParse<HrViewColumn>(doctor.Settings.HrListGrouping, true, out gr))
                    HrList.Grouping = gr;
                HrViewColumn sort;
                if (Enum.TryParse<HrViewColumn>(doctor.Settings.HrListSorting, true, out sort))
                    HrList.Sorting = sort;

                HrList.PropertyChanged += HrList_PropertyChanged;
                HrList.SaveNeeded += SaveHealthRecords;

                // сначала создаем HrList, чтобы hrManager подписался на добавление записей первым,
                // иначе HrList.SelectHealthRecord нечего выделять при добавлении записи
                holder.HealthRecordsChanged += HrsHolder_HealthRecordsChanged;
            }
        }

        private void CloseHrList()
        {
            if (HrList == null)
                return;

            var holder = HrList.holder;
            HrList.Dispose();
            HrList = null;

            holder.HealthRecordsChanged -= HrsHolder_HealthRecordsChanged;

            // сохраняем пациента и чистим записи при закрытии чего-либо (ранее в viewer.OpenedCanged мог быть переход вверх без закрытия - не сохраняет)
            saver.SaveWithCleanup(viewer.OpenedPatient);
        }

        private void ShowHeader(IHrsHolder holder)
        {
            if (Header != null)
            {
                if (Header.Holder == holder)
                    return;

                CloseHeader();
            }

            if (holder != null)
                Header = new HeaderViewModel(holder);
        }

        private void CloseHeader()
        {
            if (Header == null)
                return;

            Header.Dispose();
            Header = null;
        }

        private HealthRecord AddHr(IHrsHolder holder, bool startEdit = false)
        {
            Contract.Requires(holder != null);
            Contract.Ensures(Contract.Result<HealthRecord>().IsEmpty());

            if (HrList.holder != holder)
                Open(holder); // open holder list first

            HealthRecord hr;

            var lastHrVM = HrList.SelectedHealthRecord;
            if (HrList.SelectedHealthRecord != null && HrList.SelectedHealthRecord.healthRecord.IsEmpty())
            {
                // уже есть выбранная пустая запись
                hr = HrList.SelectedHealthRecord.healthRecord;
            }
            else
            {
                hr = holder.AddHealthRecord(AuthorityController.CurrentDoctor);
            }

            if (HrList.SelectedHealthRecord != null)
            {
                // копируем из выбранной записи
                hr.Category = lastHrVM.healthRecord.Category;
                hr.DescribedAt = lastHrVM.healthRecord.DescribedAt;
            }

            if (startEdit)
            {
                FocusHrEditor(hr, false);
            }
            return hr;
        }

        private void HrList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LastSelected")
            {
                // logger.DebugFormat("sel {0}\nlast {1}", HrList.SelectedHealthRecord, HrList.LastSelected);
                if (HrList.LastSelected != null)
                {
                    if (editorWasOpened)
                    {
                        HrEditor.Load(HrList.LastSelected.healthRecord);
                    }
                }
                else if (HrList.preserveSelected.CanEnter)
                {
                    editorWasOpened = HrEditor.HasHealthRecord;
                    HrEditor.Unload();
                }
            }
            else if (e.PropertyName == "Sorting")
            {
                doctor.Settings.HrListSorting = HrList.Sorting.ToString();
            }
            else if (e.PropertyName == "Grouping")
            {
                doctor.Settings.HrListGrouping = HrList.Grouping.ToString();
            }
        }

        private void HrsHolder_HealthRecordsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // удаляем записи в бд

                saver.Delete(e.OldItems.Cast<HealthRecord>().ToArray());
            }
        }
    }
}