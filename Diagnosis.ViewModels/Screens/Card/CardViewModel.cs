using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

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

        public void EditHr()
        {
            HealthRecordEditor.HealthRecord = HealthRecord;
        }

        internal void Open(object parameter)
        {
            Contract.Requires(parameter != null);
            var @switch = new Dictionary<Type, Action> {
                { typeof(Patient), () => viewer.OpenPatient(parameter as Patient) },
                { typeof(Course), () => viewer.OpenCourse(parameter as Course) },
                { typeof(HealthRecord),() => viewer.OpenHr(parameter as HealthRecord) },
            };

            @switch[parameter.GetType()]();
        }

        protected override void Dispose(bool disposing)
        {
            Contract.Requires(viewer.OpenedPatient != null);

            viewer.OpenedChanged -= viewer_OpenedChanged;
            Session.SaveOrUpdate(viewer.OpenedPatient);  // отписались — сохраняем пациента здесь

            viewer.ClosePatient();

            base.Dispose(disposing);
        }

        /// <summary>
        /// при открытии модели меняем соответстующую viewmodel и подписываемся
        /// на изменение выбранной вложенной сущности, чтобы открыть её модель
        /// 
        /// при закрытии разрушаем viewmodel
        /// при закрытии курса, осмотра или записи сохраняем пациента (если закрывается пациент, сохранение при разрушении CardViewModel)
        /// </summary>
        private void viewer_OpenedChanged(object sender, PatientViewer.OpeningEventArgs e)
        {
            Debug.Print("{0} {1} {2}", e.action, e.entity.GetType().Name, e.entity);

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
                                viewer.OpenedCourse = Patient.SelectedCourse.course;
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
                switch (e.action)
                {
                    case PatientViewer.OpeningAction.Open:
                        Course = new CourseViewModel(viewer.OpenedCourse);
                        Course.PropertyChanged += (s1, e1) =>
                        {
                            if (e1.PropertyName == "SelectedAppointment")
                            {
                                viewer.OpenedAppointment = Course.SelectedAppointment.To<ShortAppointmentViewModel>().appointment;
                            }
                        };
                        Patient.SelectCourse(viewer.OpenedCourse);
                        break;

                    case PatientViewer.OpeningAction.Close:
                        Course.Dispose();
                        break;
                }
            }
            else if (e.entity is Appointment)
            {
                switch (e.action)
                {
                    case PatientViewer.OpeningAction.Open:
                        Appointment = new AppointmentViewModel(viewer.OpenedAppointment);
                        Appointment.PropertyChanged += (s1, e1) =>
                        {
                            if (e1.PropertyName == "SelectedHealthRecord")
                            {
                                viewer.OpenedHealthRecord = Appointment.SelectedHealthRecord.healthRecord;
                            }
                        };
                        Course.SelectAppointment(viewer.OpenedAppointment);
                        break;

                    case PatientViewer.OpeningAction.Close:
                        Appointment.Dispose();
                        // редактор записей после смены осмотра всегда закрыт
                        editorWasOpened = false;
                        break;
                }
            }
            else if (e.entity is HealthRecord)
            {
                switch (e.action)
                {
                    case PatientViewer.OpeningAction.Open:

                        HealthRecord = new HealthRecordViewModel(viewer.OpenedHealthRecord);
                        Appointment.SelectHealthRecord(viewer.OpenedHealthRecord);

                        if (editorWasOpened)
                        {
                            HealthRecordEditor.HealthRecord = HealthRecord;
                        }
                        break;

                    case PatientViewer.OpeningAction.Close:
                        editorWasOpened = HealthRecordEditor.IsActive;
                        HealthRecordEditor.HealthRecord = null;
                        HealthRecord.Dispose();
                        break;
                }
            }

            if (e.action == PatientViewer.OpeningAction.Close && !(e.entity is Patient))
            {
                Session.SaveOrUpdate(viewer.OpenedPatient);
            }
        }
    }
}