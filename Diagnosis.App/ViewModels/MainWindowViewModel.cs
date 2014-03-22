using Diagnosis.App;
using Diagnosis.Data.Repositories;
using EventAggregator;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _loginActive;
        private LoginViewModel _loginVM;
        private PatientsListViewModel _patientsVM;
        private PatientViewModel _patientVM;
        private ICommand _logout;
        private bool _fastAddingMode;
        private bool _isPatientsVisible;

        public bool IsLoginActive
        {
            get
            {
                return _loginActive;
            }
            set
            {
                if (_loginActive != value)
                {
                    _loginActive = value;

                    OnPropertyChanged(() => IsLoginActive);
                }
            }
        }
        public bool IsPatientsVisible
        {
            get
            {
                return _isPatientsVisible;
            }
            set
            {
                if (_isPatientsVisible != value)
                {
                    _isPatientsVisible = value;
                    OnPropertyChanged(() => IsPatientsVisible);
                }
            }
        }

        public bool FastAddingMode
        {
            get
            {
                return _fastAddingMode;
            }
            set
            {
                if (_fastAddingMode != value)
                {
                    _fastAddingMode = value;
                    OnPropertyChanged(() => FastAddingMode);
                }
            }
        }

        public LoginViewModel LoginVM
        {
            get
            {
                return _loginVM;
            }
            set
            {
                if (_loginVM != value)
                {
                    _loginVM = value;
                    OnPropertyChanged(() => LoginVM);
                }
            }
        }
        public PatientsListViewModel PatientsVM
        {
            get
            {
                return _patientsVM;
            }
            set
            {
                if (_patientsVM != value)
                {
                    _patientsVM = value;
                    OnPropertyChanged(() => PatientsVM);
                }
            }
        }
        public PatientViewModel CardVM
        {
            get
            {
                return _patientVM;
            }
            set
            {
                if (_patientVM != value)
                {
                    _patientVM = value;
                    OnPropertyChanged(() => CardVM);
                }
            }
        }

        public ICommand LogoutCommand
        {
            get
            {
                return _logout ?? (_logout = new RelayCommand(
                                          () =>
                                          {
                                              IsLoginActive = true;
                                          },
                                          () => !IsLoginActive));
            }
        }
        public MainWindowViewModel()
        {
#if RELEASE
            IsLoginActive = true;
#endif

            this.Subscribe((int)EventID.CurrentPatientChanged, (e) =>
            {
                var patient = e.GetValue<PatientViewModel>(Messages.Patient);
                patient.SetDoctorVM(LoginVM.DoctorsManager.CurrentDoctor);
                CardVM = patient;
            });

            LoginVM = new LoginViewModel(new DoctorsManager(new DoctorRepository()));
            PatientsVM = new PatientsListViewModel(new PatientRepository(), EntityManagers.PropertyManager);

            LoginVM.LoggedIn += OnLoggedIn;
        }

        void OnLoggedIn(object sender, LoggedEventArgs e)
        {
            IsLoginActive = false;
            IsPatientsVisible = true;
        }
    }
}
