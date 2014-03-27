using EventAggregator;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _loginActive;
        private LoginViewModel _loginVM;
        private PatientsManager _patientsVM;
        private PatientViewModel _patientVM;
        private object _directoryVM;
        private ICommand _logout;
        private ICommand _editDiagnosisDirectory;
        private ICommand _editSymptomsDirectory;
        private bool _fastAddingMode;
        private bool _isPatientsVisible;
        private bool _isDirectoryEditing;

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

        public bool IsDirectoryEditing
        {
            get
            {
                return _isDirectoryEditing;
            }
            set
            {
                if (_isDirectoryEditing != value)
                {
                    _isDirectoryEditing = value;

                    if (value)
                    {
                        PatientsVM.NoCurrent();
                    }

                    this.Send((int)EventID.DirectoryEditingModeChanged, new DirectoryEditingModeChangedParams(value).Params);
                    OnPropertyChanged(() => IsDirectoryEditing);
                }
            }
        }
        public object DirectoryVM
        {
            get
            {
                return _directoryVM;
            }
            set
            {
                if (_directoryVM != value)
                {
                    _directoryVM = value;
                    OnPropertyChanged(() => DirectoryVM);
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

        public PatientsManager PatientsVM
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

        public ICommand EditSymptomsDirectoryCommand
        {
            get
            {
                return _editSymptomsDirectory
                    ?? (_editSymptomsDirectory = new RelayCommand(
                                          () =>
                                          {
                                              DirectoryVM = EntityManagers.SymptomsManager.Symptoms;
                                              IsDirectoryEditing = true;
                                          }));
            }
        }

        public ICommand EditDiagnosisDirectoryCommand
        {
            get
            {
                return _editDiagnosisDirectory
                    ?? (_editDiagnosisDirectory = new RelayCommand(
                                          () =>
                                          {
                                              DirectoryVM = EntityManagers.DiagnosisManager.Diagnoses;
                                              IsDirectoryEditing = true;
                                          }));
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
                SetCurrentPatient(patient);
            });

            LoginVM = new LoginViewModel(EntityManagers.DoctorsManager);
            PatientsVM = EntityManagers.PatientsManager;

            LoginVM.LoggedIn += OnLoggedIn;
        }

        private void SetCurrentPatient(PatientViewModel patient)
        {
            if (patient != null)
            {
                patient.SetDoctorVM(LoginVM.DoctorsManager.CurrentDoctor);

                IsDirectoryEditing = false;
            }
            CardVM = patient;
        }

        private void OnLoggedIn(object sender, LoggedEventArgs e)
        {
            IsLoginActive = false;
            IsPatientsVisible = true;
        }
    }
}