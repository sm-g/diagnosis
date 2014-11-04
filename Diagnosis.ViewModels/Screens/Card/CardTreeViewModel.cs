using Diagnosis.Core;
using Diagnosis.Data;
using Diagnosis.Models;
using log4net;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;

namespace Diagnosis.ViewModels.Screens
{
    public partial class CardTreeViewModel : ScreenBase
    {
        private static PatientViewer viewer; // static to hold history
        private static readonly ILog logger = LogManager.GetLogger(typeof(CardTreeViewModel));

        private HrListViewModel _hrList;
        private HrEditorViewModel _hrEditor;
        private CardItemViewModel _curHolder;
        private bool editorWasOpened;
        private bool _closeNestedOnLevelUp;

        public CardTreeViewModel(object entity, bool resetHistory = false)
        {
            if (resetHistory || viewer == null)
                viewer = new PatientViewer();

            TopCardItems = new ObservableCollection<CardItemViewModel>();
            HrEditor = new HrEditorViewModel(Session);
            HrEditor.Unloaded += (s, e) =>
            {
                // сохраняем запись при закрытии редаткора
                var hr = e.entity as HealthRecord;
                SaveHealthRecord(hr);
            };
            viewer.OpenedChanged += viewer_OpenedChanged;

            Open(entity);
        }

        public bool CloseNestedHolderOnLevelUp
        {
            get
            {
                return _closeNestedOnLevelUp;
            }
            set
            {
                if (_closeNestedOnLevelUp != value)
                {
                    _closeNestedOnLevelUp = value;
                    OnPropertyChanged(() => CloseNestedHolderOnLevelUp);
                }
            }
        }

        public CardItemViewModel CurrentHolder
        {
            get
            {
                return _curHolder;
            }
            set
            {
                if (_curHolder != value)
                {
                    _curHolder = value;

                    OnCurrentHolderChanged();

                    logger.DebugFormat("holder is {0}", value);
                    OnPropertyChanged(() => CurrentHolder);
                }
            }
        }

        /// <summary>
        /// Пациенты в корне дерева.
        /// </summary>
        public ObservableCollection<CardItemViewModel> TopCardItems
        {
            get;
            private set;
        }

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

        public HrEditorViewModel HrEditor
        {
            get
            {
                return _hrEditor;
            }
            private set
            {
                if (_hrEditor != value)
                {
                    _hrEditor = value;
                    OnPropertyChanged(() => HrEditor);
                }
            }
        }

        public RelayCommand StartCourseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    AuthorityController.CurrentDoctor.StartCourse(viewer.OpenedPatient);
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

        /// <summary>
        /// Переключает редактор для открытой записи.
        /// </summary>
        public void ToogleHrEditor()
        {
            if (HrEditor.IsActive && HrEditor.HealthRecord.healthRecord == HrList.SelectedHealthRecord.healthRecord)
            {
                HrEditor.Unload();
                // редактор записей после смены осмотра всегда закрыт
                editorWasOpened = false;
            }
            else
            {
                HrEditor.Load(HrList.SelectedHealthRecord.healthRecord);
            }
        }

        public void ResetHistory()
        {
            viewer = new PatientViewer();
        }

        internal void Open(object parameter)
        {
            Contract.Requires(parameter != null);
            logger.DebugFormat("open {0}", parameter);

            IHrsHolder holder;
            if (parameter is IHrsHolder)
            {
                holder = Session.Unproxy(parameter as IHrsHolder);
                Show(holder);
            }
            else
            {
                var hr = parameter as HealthRecord;
                holder = Session.Unproxy(hr.Holder as IHrsHolder);
                Show(holder);
                HrList.SelectHealthRecord(hr);
            }
        }

        private void Show(IHrsHolder holder)
        {
            Contract.Requires(holder != null);
            Debug.Assert(holder is Patient || holder is Course || holder is Appointment);
            logger.DebugFormat("show {0}", holder);

            // сначала создаем HrList, чтобы hrManager подписался на добавление записей первым, иначе HrList.SelectHealthRecord нечего выделять
            if (holder != GetHolderOfVm(CurrentHolder))
            {
                ShowHrsList(holder);
            }

            viewer.Open(holder);

            CurrentHolder = FindItemVmOf(holder);
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
                    return; // список может быть уже создан в Show

                HrList.Dispose(); // delete hrs here
            }

            if (holder != null)
            {
                HrList = new HrListViewModel(holder);
                HrList.PropertyChanged += HrList_PropertyChanged;
                HrList.HealthRecords.CollectionChanged += HrList_HealthRecords_CollectionChanged;
            }
        }

        private void HrList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedHealthRecord")
            {
                if (HrList.SelectedHealthRecord != null)
                {
                    editorWasOpened = HrEditor.IsActive;
                    if (editorWasOpened)
                    {
                        HrEditor.Load(HrList.SelectedHealthRecord.healthRecord);
                    }
                }
                else
                {
                    HrEditor.Unload();
                }
            }
        }

        private void HrList_HealthRecords_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Catch hr.isDeleted = true
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems.Count > 0)
                {
                    SaveHealthRecord((e.OldItems[0] as ShortHealthRecordViewModel).healthRecord);
                }
            }
        }

        private void OnCurrentHolderChanged()
        {
            // add to history
            HrEditor.Unload(); // закрываем редактор при смене активной сущности

            var holder = GetHolderOfVm(CurrentHolder);

            CurrentHolder.IsSelected = true;
            CurrentHolder.IsExpanded = true;

            ShowHrsList(holder);

            TopCardItems.ForAll(x => HightlightLastOpenedFor(x));

            if (holder is Patient)
            {
                if (CloseNestedHolderOnLevelUp)
                {
                    viewer.OpenedCourse = null;
                }
            }
            else if (holder is Course)
            {
                if (CloseNestedHolderOnLevelUp)
                {
                    viewer.OpenedAppointment = null;
                }
            }

            Title = MakeTitle();
        }

        private void HightlightLastOpenedFor(CardItemViewModel vm)
        {
            vm.Children.ForAll(x => x.IsHighlighted = false);
            var holder = viewer.GetLastOpenedFor(vm.Holder);
            var item = vm.Children.Where(x => x.Holder == holder).FirstOrDefault();
            if (item != null && item != CurrentHolder)
            {
                item.IsHighlighted = true;
            }
            vm.Children.ForAll(x => HightlightLastOpenedFor(x));
        }

        private void SaveHealthRecord(HealthRecord hr)
        {
            Session.SaveOrUpdate(hr);
            using (var t = Session.BeginTransaction())
            {
                try
                {
                    t.Commit();
                }
                catch (System.Exception e)
                {
                    t.Rollback();
                    logger.Error(e);
                }
            }
        }

        private IHrsHolder GetHolderOfVm(CardItemViewModel holderVm)
        {
            if (holderVm == null)
                return null;
            IHrsHolder holder = holderVm.Holder;
            return Session.Unproxy(holder);
        }

        private CardItemViewModel FindItemVmOf(IHrsHolder holder)
        {
            holder = Session.Unproxy(holder);
            CardItemViewModel vm;
            foreach (var item in TopCardItems)
            {
                if (item.Holder == holder)
                    return item;
                vm = item.AllChildren.Where(x => x.Holder == holder).FirstOrDefault();
                if (vm != null)
                    return vm;
            }
            return null;
        }

        private string MakeTitle()
        {
            string delim = " — ";
            string result = string.Format("{0} {1}", viewer.OpenedPatient.Label, NameFormatter.GetShortName(viewer.OpenedPatient));

            var holder = GetHolderOfVm(CurrentHolder);

            if (holder is Course)
            {
                result += delim + "курс " + DateFormatter.GetIntervalString(viewer.OpenedCourse.Start, viewer.OpenedCourse.End);
            }
            else if (holder is Appointment)
            {
                result += delim + "курс " + DateFormatter.GetIntervalString(viewer.OpenedCourse.Start, viewer.OpenedCourse.End);
                result += delim + "осмотр " + DateFormatter.GetDateString(viewer.OpenedAppointment.DateAndTime);
            }
            return result;
        }

        /// <summary>
        /// при открытии модели меняем соответствующую viewmodel и добавляем её в список hrsholders
        /// показываем holder
        ///
        /// при закрытии разрушаем viewmodel и убираем из списка hrsholders,
        /// сохраняем пациента
        ///
        /// опционально Если в открываемых первый раз курсе нет осмотров или в осмотре нет записей, добавляет их.
        /// </summary>
        private void viewer_OpenedChanged(object sender, PatientViewer.OpeningEventArgs e)
        {
            Contract.Requires(e.entity is IHrsHolder);
            logger.DebugFormat("{0} {1} {2}", e.action, e.entity.GetType().Name, e.entity);

            if (e.action == PatientViewer.OpeningAction.Close)
            {
                SaveAll();
            }

            CardItemViewModel itemVm;
            if (e.entity is Patient)
            {
                var patient = e.entity as Patient;
                switch (e.action)
                {
                    case PatientViewer.OpeningAction.Open:
                        itemVm = new CardItemViewModel(patient, Session);

                        patient.HealthRecordsChanged += HrsHolder_HealthRecordsChanged;
                        patient.CoursesChanged += patient_CoursesChanged;

                        TopCardItems.Add(itemVm);

                        break;

                    case PatientViewer.OpeningAction.Close:
                        patient.HealthRecordsChanged -= HrsHolder_HealthRecordsChanged;
                        patient.CoursesChanged -= patient_CoursesChanged;

                        itemVm = FindItemVmOf(patient);
                        TopCardItems.Remove(itemVm);
                        itemVm.Dispose();
                        break;
                }
            }
            else if (e.entity is Course)
            {
                var course = e.entity as Course;
                switch (e.action)
                {
                    case PatientViewer.OpeningAction.Open:
                        itemVm = FindItemVmOf(course);
                        itemVm.IsExpanded = true;

                        course.HealthRecordsChanged += HrsHolder_HealthRecordsChanged;
                        course.AppointmentsChanged += course_AppointmentsChanged;

                        //if (course.Appointments.Count() == 0)
                        //{
                        //    course.AddAppointment(course.LeadDoctor); // добавляем первый осмотр
                        //}
                        break;

                    case PatientViewer.OpeningAction.Close:
                        course.HealthRecordsChanged -= HrsHolder_HealthRecordsChanged;
                        course.AppointmentsChanged -= course_AppointmentsChanged;

                        itemVm = FindItemVmOf(course);
                        itemVm.IsExpanded = false;
                        break;
                }
            }
            else if (e.entity is Appointment)
            {
                var app = e.entity as Appointment;
                switch (e.action)
                {
                    case PatientViewer.OpeningAction.Open:
                        itemVm = FindItemVmOf(app);
                        itemVm.IsExpanded = true;

                        app.HealthRecordsChanged += HrsHolder_HealthRecordsChanged;

                        //if (app.HealthRecords.Count() == 0)
                        //{
                        //    app.AddHealthRecord(); // добавляем первую запись
                        //}
                        break;

                    case PatientViewer.OpeningAction.Close:
                        app.HealthRecordsChanged -= HrsHolder_HealthRecordsChanged;
                        itemVm = FindItemVmOf(app);
                        itemVm.IsExpanded = false;

                        break;
                }
            }
        }

        /// <summary>
        /// Сохраняем пациента, его курсы, осмотры и все записи.
        /// </summary>
        private void SaveAll()
        {
            Session.SaveOrUpdate(viewer.OpenedPatient);
            using (var t = Session.BeginTransaction())
            {
                try
                {
                    t.Commit();
                }
                catch (System.Exception e)
                {
                    t.Rollback();
                    logger.Error(e);
                }
            }
        }
        private void HrsHolder_HealthRecordsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // редактируем добавленную запись
                var hr = (HealthRecord)e.NewItems[e.NewItems.Count - 1];
                HrList.SelectHealthRecord(hr);
                HrEditor.Load(hr);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // удаляем записи в бд
                SaveAll();
            }
        }

        private void patient_CoursesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // при добавлении курса открываем его
                Show((Course)e.NewItems[e.NewItems.Count - 1]);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // при удалении курса открываем курс рядом с удаленным
                //var i = e.OldStartingIndex;
                //if (OpenedPatient.Courses.Count() <= i)
                //    i--;
                //if (OpenedPatient.Courses.Count() > 0)
                //{
                //    OpenedCourse = OpenedPatient.Courses.ElementAt(i);
                //}
            }
        }

        private void course_AppointmentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // при добавлении осмотра открываем его
                Show((Appointment)e.NewItems[e.NewItems.Count - 1]);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // при удалении осмотра открываем последний осмотр
                //if (OpenedAppointment == null && OpenedCourse.Appointments.Count() > 0)
                //{
                //    OpenedAppointment = OpenedCourse.Appointments.LastOrDefault();
                //}
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Contract.Assume(viewer.OpenedPatient != null);
            }

            try
            {
                if (disposing)
                {
                    HrEditor.Dispose();
                    HrList.Dispose(); // удаляются все записи

                    viewer.ClosePatient(); // сохраняем пациента при закрытии
                    viewer.OpenedChanged -= viewer_OpenedChanged;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}