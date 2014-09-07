using Diagnosis.Core;
using Diagnosis.Models;
using EventAggregator;

namespace Diagnosis.ViewModels
{
    public class CardViewModel : SessionVMBase
    {
        public static PatientViewer viewer = new PatientViewer();

        private AppointmentViewModel _appointment;

        private CourseViewModel _course;

        private HrEditorViewModel _hrEditor;

        private PatientViewModel _patient;

        public CardViewModel()
        {
            HealthRecordEditor = new HrEditorViewModel(session);

            viewer.Session = session;
            viewer.PropertyChanged += viewer_PropertyChanged;
        }

        public AppointmentViewModel Appointment
        {
            get
            {
                return _appointment;
            }
            set
            {
                if (_appointment != value)
                {
                    _appointment = value;
                    OnPropertyChanged(() => Appointment);
                }
            }
        }

        public CourseViewModel Course
        {
            get
            {
                return _course;
            }
            set
            {
                if (_course != value)
                {
                    _course = value;
                    OnPropertyChanged(() => Course);
                }
            }
        }

        public HrEditorViewModel HealthRecordEditor
        {
            get
            {
                return _hrEditor;
            }
            set
            {
                if (_hrEditor != value)
                {
                    _hrEditor = value;
                    OnPropertyChanged(() => HealthRecordEditor);
                }
            }
        }

        public PatientViewModel Patient
        {
            get
            {
                return _patient;
            }
            set
            {
                if (_patient != value)
                {
                    _patient = value;
                    OnPropertyChanged(() => Patient);
                }
            }
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

                    }
                    else
                    {

                    }
                    break;
                default:
                    break;
            }
        }
    }
}