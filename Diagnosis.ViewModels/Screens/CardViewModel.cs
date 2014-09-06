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
            HrEditor = new HrEditorViewModel(session);

            this.Subscribe(Events.PatientCreated, (e) =>
            {
                var pat = e.GetValue<PatientViewModel>(MessageKeys.Patient);
                viewer.OpenPatient(pat);
            });
            this.Subscribe(Events.OpenHealthRecord, (e) =>
            {
                var hr = e.GetValue<HealthRecord>(MessageKeys.HealthRecord);

                viewer.OpenHr(hr);
            });
            this.Subscribe(Events.PatientAdded, (e) =>
            {
                var pat = e.GetValue<PatientViewModel>(MessageKeys.Patient);
                viewer.OpenPatient(pat);
            });

            this.Subscribe(Events.OpenPatient, (e) =>
            {
                var pat = e.GetValue<PatientViewModel>(MessageKeys.Patient);
                viewer.OpenPatient(pat);
            });
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

        public HrEditorViewModel HrEditor
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
                    OnPropertyChanged(() => HrEditor);
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
    }
}