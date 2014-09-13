using Diagnosis.Models;
using System;
using System.Collections.Generic;
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

        public CardViewModel(object entity)
            : this()
        {
            Open(entity);
        }

        private CardViewModel()
        {
            HealthRecordEditor = new HrEditorViewModel(Session);
            viewer.PropertyChanged += viewer_PropertyChanged;
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

            viewer.PropertyChanged -= viewer_PropertyChanged;
            Session.SaveOrUpdate(viewer.OpenedPatient);  // отписались — сохраняем пациента здесь

            viewer.ClosePatient();

            base.Dispose(disposing);
        }

        private void viewer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // при открытии модели меняем соответстующую viewmodel и подписываемся 
                // на изменение выбранной вложенной сущности, чтобы открыть её модель

                // при закрытии курса, осмотра или записи сохраняем пациента (если закрывается пациент, сохранение при разрушении CardViewModel)
                case "OpenedPatient":
                    if (viewer.OpenedPatient != null)
                    {
                        Patient = new PatientViewModel(viewer.OpenedPatient);
                        Patient.PropertyChanged += (s1, e1) =>
                        {
                            if (e1.PropertyName == "SelectedCourse")
                            {
                                viewer.OpenedCourse = Patient.SelectedCourse.course;
                            }
                        };
                    }
                    else
                    {
                        Patient = null;
                    }
                    break;

                case "OpenedCourse":
                    if (viewer.OpenedCourse != null)
                    {
                        Course = new CourseViewModel(viewer.OpenedCourse);
                        Course.PropertyChanged += (s1, e1) =>
                        {
                            if (e1.PropertyName == "SelectedAppointment")
                            {
                                viewer.OpenedAppointment = Course.SelectedAppointment.To<ShortAppointmentViewModel>().appointment;
                            }
                        };
                        Patient.SelectCourse(viewer.OpenedCourse);
                    }
                    else
                    {
                        Course = null;
                        Session.SaveOrUpdate(viewer.OpenedPatient);
                    }
                    break;

                case "OpenedAppointment":
                    if (viewer.OpenedAppointment != null)
                    {
                        Appointment = new AppointmentViewModel(viewer.OpenedAppointment);
                        Appointment.PropertyChanged += (s1, e1) =>
                        {
                            if (e1.PropertyName == "SelectedHealthRecord")
                            {
                                viewer.OpenedHealthRecord = Appointment.SelectedHealthRecord.healthRecord;
                            }
                        };
                        Course.SelectAppointment(viewer.OpenedAppointment);
                    }
                    else
                    {
                        Appointment = null;
                        // закрываем редактор записей при смене осмотра
                        HealthRecordEditor.HealthRecord = null;

                        Session.SaveOrUpdate(viewer.OpenedPatient);
                    }
                    break;

                case "OpenedHealthRecord":
                    if (viewer.OpenedHealthRecord != null)
                    {
                        HealthRecord = new HealthRecordViewModel(viewer.OpenedHealthRecord);

                        Appointment.SelectHealthRecord(viewer.OpenedHealthRecord);

                        if (viewer.OpenedHealthRecord.Appointment == viewer.OpenedAppointment)
                        {
                            HealthRecordEditor.HealthRecord = HealthRecord;
                        }
                    }
                    else
                    {
                        HealthRecord = null;
                        HealthRecordEditor.HealthRecord = null;

                        Session.SaveOrUpdate(viewer.OpenedPatient);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}