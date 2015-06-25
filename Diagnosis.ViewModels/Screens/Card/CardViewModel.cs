using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
using EventAggregator;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public partial class CardViewModel : ScreenBaseViewModel
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(CardViewModel));
        static HierViewer<Patient, Course, Appointment, IHrsHolder> viewer;
        readonly HrEditorViewModel _hrEditor;
        readonly CardNavigator _navigator;
        HrListViewModel _hrList;
        HeaderViewModel _header;
        bool editorWasOpened;
        IHrsHolder deletingHolder;
        Action pendingAction;

        public CardViewModel(bool resetHistory = false)
        {
            if (IsInDesignMode) return;
            if (resetHistory || viewer == null)
                ResetHistory();

            Contract.Assume(AuthorityController.CurrentDoctor != null);

            _navigator = new CardNavigator(viewer);
            _hrEditor = new HrEditorViewModel(Session);

            Navigator.CurrentChanged += (s, e) =>
            {
                // add to history

                // закрываем редактор при смене активной сущности
                HrEditor.Unload();

                var holder = e.arg != null ? (e.arg as CardItemViewModel).Holder : null;

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
                if (hr.Doctor == AuthorityController.CurrentDoctor)  // добавлять только если врач редактировал свою запись?
                    AuthorityController.CurrentDoctor.AddWords(hr.Words);
                Session.DoSave(hr);
            };
            HrEditor.Closing += (s, e) =>
            {
                // закрыт по команде — переходим к списку записей
                // logger.DebugFormat("hreditor closed, listfocused = {0}", HrList.IsFocused);
                HrList.IsFocused = true;
                // restore selected?
            };

            emh.Add(new EventMessageHandler[] {
                this.Subscribe(Event.DeleteHolder, (e) =>
                {
                    var holder = e.GetValue<IHrsHolder>(MessageKeys.Holder);
                    DeleteHolder(holder);

                    Contract.Assume(HrList == null || HrList.holder != holder);
                }),
                this.Subscribe(Event.EntityDeleted, (e) =>
                {
                    var entity = e.GetValue<IEntity>(MessageKeys.Entity);
                    if (entity is IHrsHolder)
                    {
                        // дожидаемся конца транзации удаления перед тем как менять экран и сохранять врача
                        if (deletingHolder == null)
                            OnHolderDeleted(entity as IHrsHolder);
                        else
                            pendingAction = () => OnHolderDeleted(entity as IHrsHolder);
                    }
                }),
                this.Subscribe(Event.AddHr, (e) =>
                {
                    var holder = e.GetValue<IHrsHolder>(MessageKeys.Holder);
                    var startEdit = e.GetValue<bool>(MessageKeys.Boolean);

                    AddHr(holder, startEdit);
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

        public CardNavigator Navigator { get { return _navigator; } }

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
                    viewer.OpenedRoot.AddCourse(AuthorityController.CurrentDoctor);
                });
            }
        }

        public RelayCommand AddAppointmentCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    viewer.OpenedMiddle.AddAppointment(AuthorityController.CurrentDoctor);
                },
                () => viewer.OpenedMiddle != null && viewer.OpenedMiddle.End == null);
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
                // Contract.Assume(HrEditor.HealthRecord.healthRecord == HrList.LastSelected.healthRecord);
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
        public void StartEditHr(HealthRecord hr, bool addToSelected = true)
        {
            Contract.Requires(hr != null);
            Contract.Assume(hr.Holder == HrList.holder);

            //logger.DebugFormat("StartEditHr {0}", hr);
            HrList.SelectHealthRecord(hr, addToSelected);
            HrEditor.Load(hr);
            HrEditor.Autocomplete.StartEdit();
        }

        public void ResetHistory()
        {
            viewer = new HierViewer<Patient, Course, Appointment, IHrsHolder>(
                c => c.Patient,
                a => a.Course,
                p => p.GetOrderedCourses(),
                c => c.GetOrderedAppointments());
            HrListViewModel.ResetHistory();
        }

        /// <summary>
        /// Открывает держателя или выделяет записи.
        /// </summary>
        public void Open(object toOpen, bool lastAppOrCourse = false)
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
            var was = viewer.AutoOpenChild;
            if (lastAppOrCourse)
                viewer.AutoOpenChild = true;

            Navigator.NavigateTo(holder);

            viewer.AutoOpenChild = was;
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

        /// <summary>
        /// Показвает записи активной сущности.
        /// Закрывает список записей, если передан null.
        /// </summary>
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
                HrList = new HrListViewModel(holder, Session);
                var doctor = AuthorityController.CurrentDoctor;
                HrViewColumn gr;
                if (Enum.TryParse<HrViewColumn>(doctor.Settings.HrListGrouping, true, out gr))
                    HrList.Grouping = gr;
                HrViewColumn sort;
                if (Enum.TryParse<HrViewColumn>(doctor.Settings.HrListSorting, true, out sort))
                    HrList.Sorting = sort;

                HrList.SelectLastSelectedForHolder();

                HrList.PropertyChanged += HrList_PropertyChanged;
                HrList.HrsSaved += (s, e) =>
                {
                    if (!HrEditor.HasHealthRecord)
                    {
                        HrList.IsFocused = true;
                    }
                };
            }
        }

        private void CloseHrList()
        {
            if (HrList == null)
                return;

            var holder = HrList.holder;

            HrList.Dispose();
            HrList = null;

            //do not save holder after deleting it
            if (deletingHolder == holder)
                return;

            var hrs = RemoveEmptyHrs(holder).ToArray();
            Session.DoSave(holder);
        }

        private static IEnumerable<HealthRecord> RemoveEmptyHrs(IHrsHolder holder)
        {
            Contract.Requires(holder != null);
            Contract.Ensures(holder.HealthRecords.All(x => !x.IsEmpty()));

            var emptyHrs = holder.HealthRecords.Where(hr => hr.IsEmpty()).ToList();
            emptyHrs.ForEach(hr => holder.RemoveHealthRecord(hr));
            return emptyHrs;
        }

        internal void DeleteHolder(IHrsHolder holder)
        {
            Contract.Assume(deletingHolder == null);

            deletingHolder = holder;

            if (holder is Course)
            {
                var course = holder as Course;
                course.Patient.RemoveCourse(course);
            }
            else if (holder is Appointment)
            {
                var app = holder as Appointment;
                app.Course.RemoveAppointment(app);
            }
            Session.DoDelete(holder);

            ExecutePendingActions();
        }

        private void ExecutePendingActions()
        {
            if (pendingAction == null)
                return;
            pendingAction();
            pendingAction = null;
        }

        private void OnHolderDeleted(IHrsHolder holder)
        {
            Contract.Assume(deletingHolder != null);

            viewer.RemoveFromHistory(holder);

            if (holder is Patient)
            {
                Navigator.RemoveRoot(holder as Patient);
                if (Navigator.TopItems.Count == 0)
                    OnLastItemRemoved();
            }

            // holder may be child of deleting
            if (deletingHolder == holder)
                deletingHolder = null;
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

        private void AddHr(IHrsHolder holder, bool startEdit)
        {
            Contract.Requires(holder != null);
            Contract.Ensures(HrList.holder == holder);

            // open holder list first
            if (HrList.holder != holder)
                Open(holder);

            var hr = HrList.AddHr();

            if (startEdit)
            {
                StartEditHr(hr, false);
            }
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
                var doctor = AuthorityController.CurrentDoctor;
                doctor.Settings.HrListSorting = HrList.Sorting.ToString();
            }
            else if (e.PropertyName == "Grouping")
            {
                var doctor = AuthorityController.CurrentDoctor;
                doctor.Settings.HrListGrouping = HrList.Grouping.ToString();
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

                    Session.DoSave(AuthorityController.CurrentDoctor);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}