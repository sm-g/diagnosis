using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class CardViewModel : SessionVMBase
    {
        protected static PatientViewer viewer = new PatientViewer(); // static to hold history

        private PatientViewModel _patient;
        private CourseViewModel _course;
        private AppointmentViewModel _appointment;
        private HealthRecordViewModel _hr;
        private HrEditorViewModel _hrEditor;
        private bool editorWasOpened;

        public CardViewModel(object entity)
            : this()
        {
            Open(entity);
        }

        private CardViewModel()
        {
            HealthRecordEditor = new HrEditorViewModel(Session);
            viewer.OpenedChanged += viewer_OpenedChanged;
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

        public CourseViewModel Course
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

        public HealthRecordViewModel HealthRecord
        {
            get
            {
                return _hr;
            }
            set
            {
                if (_hr != value)
                {
                    _hr = value;
                    OnPropertyChanged(() => HealthRecord);
                }
            }
        }

        public HrEditorViewModel HealthRecordEditor
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
                    OnPropertyChanged(() => HealthRecordEditor);
                }
            }
        }

        /// <summary>
        /// Переключает редактор для открытой записи.
        /// </summary>
        public void ToogleHrEditor()
        {
            if (HealthRecordEditor.IsActive && HealthRecordEditor.HealthRecord.healthRecord == HealthRecord.healthRecord)
            {
                HealthRecordEditor.Unload();
            }
            else
            {
                HealthRecordEditor.Load(HealthRecord.healthRecord);
            }
        }

        internal void Open(object parameter)
        {
            Contract.Requires(parameter != null);
            var @switch = new Dictionary<Type, Action> {
                { typeof(Patient), () => viewer.OpenPatient(parameter as Patient) },
                { typeof(Course), () => viewer.OpenCourse(parameter as Course) },
                { typeof(HealthRecord),() => viewer.OpenAppHr(parameter as HealthRecord) },
            };

            @switch[parameter.GetType()]();
        }

        protected override void Dispose(bool disposing)
        {
            Contract.Assume(viewer.OpenedPatient != null);
            try
            {
                if (disposing)
                {
                    viewer.ClosePatient(); // сохраняем пациента при закрытии
                    viewer.OpenedChanged -= viewer_OpenedChanged;

                    HealthRecordEditor.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// синхронизация:
        /// при открытии модели меняем соответстующую viewmodel и подписываемся
        /// на изменение выбранной вложенной сущности, чтобы открыть её модель
        ///
        /// при закрытии разрушаем viewmodel
        /// 
        /// Если в открываемых первый раз курсе нет осмотров или в осмотре нет записей, добавляет их.
        ///
        /// при закрытии курса, осмотра или записи сохраняем пациента (если закрывается пациент, сохранение при разрушении CardViewModel)
        /// </summary>
        private void viewer_OpenedChanged(object sender, PatientViewer.OpeningEventArgs e)
        {
            Debug.Print("{0} {1} {2}", e.action, e.entity.GetType().Name, e.entity);

            // сохраняем все изменения при закрытии пациента, курса, осмотра, записи
            if (e.action == PatientViewer.OpeningAction.Close)
            {
                Session.SaveOrUpdate(viewer.OpenedPatient);
            }

            if (e.entity is Patient)
            {
                switch (e.action)
                {
                    case PatientViewer.OpeningAction.Open:
                        Patient = new PatientViewModel(viewer.OpenedPatient);
                        Patient.PropertyChanged += (s1, e1) =>
                        {
                            if (e1.PropertyName == "SelectedCourse")
                            {
                                if (Patient.SelectedCourse != null)
                                    viewer.OpenedCourse = Patient.SelectedCourse.course;
                                else
                                    viewer.OpenedCourse = null;
                            }
                        };
                        break;

                    case PatientViewer.OpeningAction.Close:
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
                        Course = new CourseViewModel(course);
                        Course.PropertyChanged += (s1, e1) =>
                        {
                            if (e1.PropertyName == "SelectedAppointment")
                            {
                                if (Course.SelectedAppointment != null)
                                    viewer.OpenedAppointment = Course.SelectedAppointment.To<ShortAppointmentViewModel>().appointment;
                                else
                                    viewer.OpenedAppointment = null;
                            }
                        };
                        Patient.SelectCourse(course);
                        if (course.Appointments.Count() == 0)
                        {
                            course.AddAppointment(course.LeadDoctor); // добавляем первый осмотр
                        }
                        break;

                    case PatientViewer.OpeningAction.Close:
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
                        app.HealthRecordsChanged += app_HealthRecordsChanged;
                        Appointment.PropertyChanged += (s1, e1) =>
                        {
                            if (e1.PropertyName == "SelectedHealthRecord")
                            {
                                if (Appointment.SelectedHealthRecord != null)
                                    viewer.OpenedHealthRecord = Appointment.SelectedHealthRecord.healthRecord;
                                else
                                    viewer.OpenedHealthRecord = null;
                            }
                        };
                        Course.SelectAppointment(app);

                        if (app.HealthRecords.Count() == 0)
                        {
                            app.AddHealthRecord(); // добавляем первую запись
                        }
                        break;

                    case PatientViewer.OpeningAction.Close:
                        app.HealthRecordsChanged -= app_HealthRecordsChanged;

                        Appointment.MakeDeletions();
                        Appointment.Dispose();
                        // редактор записей после смены осмотра всегда закрыт
                        editorWasOpened = false;

                        // соханяем все изменения
                        using (var t = Session.BeginTransaction())
                        {
                            t.Commit();
                        }

                        break;
                }
            }
            else if (e.entity is HealthRecord)
            {
                var hr = e.entity as HealthRecord;
                switch (e.action)
                {
                    case PatientViewer.OpeningAction.Open:
                        HealthRecord = new HealthRecordViewModel(hr);
                        Appointment.SelectHealthRecord(hr);

                        if (editorWasOpened)
                        {
                            HealthRecordEditor.Load(hr);
                        }
                        break;

                    case PatientViewer.OpeningAction.Close:
                        editorWasOpened = HealthRecordEditor.IsActive;
                        HealthRecordEditor.Unload();
                        HealthRecord.Dispose();
                        break;
                }
            }
        }

        private void app_HealthRecordsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // редактируем добавленную запись
                var hr = (HealthRecord)e.NewItems[e.NewItems.Count - 1];
                viewer.OpenAppHr(hr);
                HealthRecordEditor.Load(hr);
            }
        }
    }
}