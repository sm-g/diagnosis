using Diagnosis.Models;
using log4net;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Windows;

namespace Diagnosis.ViewModels
{
    public partial class CardViewModel : SessionVMBase
    {
        private static PatientViewer viewer = new PatientViewer(); // static to hold history
        public static readonly ILog logger = LogManager.GetLogger(typeof(CardViewModel));

        private PatientViewModel _patient;
        private CourseViewModel1 _course;
        private AppointmentViewModel _appointment;
        private HrListViewModel _hrList;
        private HrEditorViewModel _hrEditor;
        private ViewModelBase _curHolder;
        private bool editorWasOpened;
        private bool _closeNestedOnLevelUp;

        public CardViewModel(object entity)
            : this()
        {
            Open(entity);
        }

        public CardViewModel()
        {
            HrEditor = new HrEditorViewModel(Session);
            HrEditor.Unloaded += (s, e) =>
            {
                var hr = e.entity as HealthRecord;
                SaveHealthRecord(hr);
            };
            viewer.OpenedChanged += viewer_OpenedChanged;
            HrsHolders = new ObservableCollection<ViewModelBase>();
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

        public PatientViewModel Patient
        {
            get
            {
                return _patient;
            }
            private set
            {
                if (_patient != value)
                {
                    _patient = value;
                    OnPropertyChanged(() => Patient);
                }
            }
        }

        public CourseViewModel1 Course
        {
            get
            {
                return _course;
            }
            private set
            {
                if (_course != value)
                {
                    _course = value;
                    OnPropertyChanged(() => Course);
                }
            }
        }

        public AppointmentViewModel Appointment
        {
            get
            {
                return _appointment;
            }
            private set
            {
                if (_appointment != value)
                {
                    _appointment = value;
                    OnPropertyChanged(() => Appointment);
                }
            }
        }

        public ViewModelBase CurrentHolder
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

        public ObservableCollection<ViewModelBase> HrsHolders
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

        internal void Open(object parameter)
        {
            Contract.Requires(parameter != null);
            logger.DebugFormat("open {0}", parameter);

            IHrsHolder holder;
            if (parameter is IHrsHolder)
            {
                holder = (IHrsHolder)Session.GetSessionImplementation().PersistenceContext.Unproxy(parameter);
                Show(holder);
            }
            else
            {
                var hr = parameter as HealthRecord;
                holder = (IHrsHolder)Session.GetSessionImplementation().PersistenceContext.Unproxy(hr.Holder);
                Show(holder);
                HrList.SelectHealthRecord(hr);
            }
        }

        private void Show(IHrsHolder holder)
        {
            Contract.Requires(holder != null);
            logger.DebugFormat("show {0}", holder);

            // сначала создаем HrList, чтобы hrManager подписался на добавление записей первым, иначе HrList.SelectHealthRecord нечего выделять
            if (holder != GetHolderOfVm(CurrentHolder))
            {
                ShowHrsList(holder);
            }

            viewer.Open(holder);

            if (holder is Patient)
            {
                CurrentHolder = Patient;
            }
            else if (holder is Course)
            {
                CurrentHolder = Course;
            }
            else if (holder is Appointment)
            {
                CurrentHolder = Appointment;
            }
        }

        /// <summary>
        /// Показвает записи активной сущности.
        /// </summary>
        /// <param name="holder"></param>
        private void ShowHrsList(IHrsHolder holder)
        {
            if (HrList != null)
            {
                HrList.Dispose();
            }

            HrList = new HrListViewModel(holder);
            HrList.PropertyChanged += HrList_PropertyChanged;
            HrList.HealthRecords.CollectionChanged += HrList_HealthRecords_CollectionChanged;
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
            ShowHrsList(holder);

            if (holder is Patient)
            {
                Patient.SelectCourse(viewer.GetLastOpenedFor(holder as Patient));

                if (CloseNestedHolderOnLevelUp)
                {
                    viewer.OpenedCourse = null;
                    viewer.OpenedAppointment = null;
                }
            }
            else if (holder is Course)
            {
                Course.SelectAppointment(viewer.GetLastOpenedFor(holder as Course));

                if (CloseNestedHolderOnLevelUp)
                {
                    viewer.OpenedAppointment = null;
                }
            }
        }

        private void SaveHealthRecord(HealthRecord hr)
        {
            Session.SaveOrUpdate(hr);
            using (var t = Session.BeginTransaction())
            {
                t.Commit();
            }
        }

        private IHrsHolder GetHolderOfVm(ViewModelBase holderVm)
        {
            IHrsHolder holder = null;
            if (holderVm is AppointmentViewModel)
            {
                holder = Appointment.appointment;
            }
            else if (holderVm is CourseViewModel1)
            {
                holder = Course.course;
            }
            else if (holderVm is PatientViewModel)
            {
                holder = Patient.patient;
            }
            return holder;
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
                Session.SaveOrUpdate(viewer.OpenedPatient);
                using (var t = Session.BeginTransaction())
                {
                    t.Commit();
                }
            }

            if (e.entity is Patient)
            {
                var patient = e.entity as Patient;
                switch (e.action)
                {
                    case PatientViewer.OpeningAction.Open:
                        Patient = new PatientViewModel(patient);

                        patient.HealthRecordsChanged += HrsHolder_HealthRecordsChanged;
                        patient.CoursesChanged += patient_CoursesChanged;

                        HrsHolders.Add(Patient);

                        break;

                    case PatientViewer.OpeningAction.Close:
                        patient.HealthRecordsChanged -= HrsHolder_HealthRecordsChanged;
                        patient.CoursesChanged -= patient_CoursesChanged;

                        HrsHolders.Remove(Patient);
                        Patient.Dispose();
                        break;
                }
            }
            else if (e.entity is Course)
            {
                var course = e.entity as Course;
                switch (e.action)
                {
                    case PatientViewer.OpeningAction.Open:
                        Course = new CourseViewModel1(course);

                        HrsHolders.Add(Course);

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

                        HrsHolders.Remove(Course);

                        Course.Dispose();
                        break;
                }
            }
            else if (e.entity is Appointment)
            {
                var app = e.entity as Appointment;
                switch (e.action)
                {
                    case PatientViewer.OpeningAction.Open:
                        Appointment = new AppointmentViewModel(app);
                        HrsHolders.Add(Appointment);

                        app.HealthRecordsChanged += HrsHolder_HealthRecordsChanged;

                        //if (app.HealthRecords.Count() == 0)
                        //{
                        //    app.AddHealthRecord(); // добавляем первую запись
                        //}
                        break;

                    case PatientViewer.OpeningAction.Close:
                        app.HealthRecordsChanged -= HrsHolder_HealthRecordsChanged;
                        HrsHolders.Remove(Appointment);

                        Appointment.Dispose();

                        break;
                }
            }
        }

        private void HrsHolder_HealthRecordsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // редактируем добавленную запись
                var hr = (HealthRecord)e.NewItems[e.NewItems.Count - 1];
                // HrList.AddHealthRecordCommand
                HrList.SelectHealthRecord(hr);
                HrEditor.Load(hr);
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
                    HrList.Dispose(); // сохраняются и удаляются все записи

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