using Diagnosis.Core;
using Diagnosis.Models;
using EventAggregator;
using NHibernate;

namespace Diagnosis.ViewModels
{
    public class CardViewModel : SessionVMBase
    {
        public static PatientViewer viewer = new PatientViewer();
        public static ISession Session;

        private PatientViewModel _patient;
        private CourseViewModel _course;
        private AppointmentViewModel _appointment;
        private HrEditorViewModel _hrEditor;

        public CardViewModel()
        {
            Session = base.Session;
            HealthRecordEditor = new HrEditorViewModel(Session);
            viewer.Session = Session;
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

        private HealthRecordViewModel _hr;
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

        protected override void Dispose(bool disposing)
        {
            viewer.PropertyChanged -= viewer_PropertyChanged;
            base.Dispose(disposing);
        }

        void viewer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
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
                    }
                    break;
                case "OpenedHealthRecord":
                    if (viewer.OpenedHealthRecord != null)
                    {
                        HealthRecord = new HealthRecordViewModel(viewer.OpenedHealthRecord);

                        Appointment.SelectHealthRecord(viewer.OpenedHealthRecord);
                    }
                    else
                    {
                        HealthRecord = null;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}