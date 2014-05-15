using EventAggregator;
using System.Windows.Input;
using Diagnosis.Data.Repositories;

namespace Diagnosis.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _loginActive;
        private LoginViewModel _loginVM;
        private PatientViewModel _patientVM;
        private ICommand _logout;
        private ICommand _editDiagnosisDirectory;
        private ICommand _editSymptomsDirectory;
        private bool _fastAddingMode;
        private bool _isPatientsVisible;
        private bool _isWordsEditing;
        private object _directoryExplorer;

        #region Flags
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
                    if (value)
                    {
                        CurrentScreen = LoginVM;
                    }
                    else
                    {
                        CurrentScreen = EntityManagers.PatientsManager.CurrentPatient;
                    }

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

        public bool IsWordsEditing
        {
            get
            {
                return _isWordsEditing;
            }
            set
            {
                if (_isWordsEditing != value)
                {
                    _isWordsEditing = value;

                    if (value)
                    {
                        PatientsManager.RemoveCurrent();
                        CurrentScreen = EntityManagers.WordsManager;
                    }

                    this.Send((int)EventID.WordsEditingModeChanged, new DirectoryEditingModeChangedParams(value).Params);
                    OnPropertyChanged(() => IsWordsEditing);
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

        private bool _searchTester;
        public bool SearchTesterState
        {
            get
            {
                return _searchTester;
            }
            set
            {
                if (_searchTester != value)
                {
                    _searchTester = value;
                    if (value)
                    {
                        CurrentScreen = new SearchTester();
                    }
                    else
                    {
                        CurrentScreen = EntityManagers.PatientsManager.CurrentPatient;
                    }
                    OnPropertyChanged(() => SearchTesterState);
                }
            }
        }
        #endregion

        #region ViewModels


        private ViewModelBase _currentScreen;
        public ViewModelBase CurrentScreen
        {
            get
            {
                return _currentScreen;
            }
            set
            {
                if (_currentScreen != value)
                {
                    _currentScreen = value;
                    OnPropertyChanged(() => CurrentScreen);
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

        public PatientsManager PatientsManager
        {
            get
            {
                return EntityManagers.PatientsManager;
            }
        }

        public object DirectoryExplorer
        {
            get
            {
                return _directoryExplorer;
            }
            set
            {
                if (_directoryExplorer != value)
                {
                    _directoryExplorer = value;
                    OnPropertyChanged(() => DirectoryExplorer);
                }
            }
        }
        #endregion

        #region Commands

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
                                              IsWordsEditing = true;
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
                                              DirectoryExplorer = new HierarchicalExplorer<DiagnosisViewModel>(EntityManagers.DiagnosisManager.Diagnoses);
                                          }));
            }
        }
        #endregion

        public MainWindowViewModel()
        {
            this.Subscribe((int)EventID.CurrentPatientChanged, (e) =>
            {
                var patient = e.GetValue<PatientViewModel>(Messages.Patient);
                SetCurrentPatient(patient);
            });

            LoginVM = new LoginViewModel(EntityManagers.DoctorsManager);
            LoginVM.LoggedIn += OnLoggedIn;

            CurrentScreen = LoginVM;
        }

        private void SetCurrentPatient(PatientViewModel patient)
        {
            if (patient != null)
            {
                patient.SetDoctorVM(LoginVM.DoctorsManager.CurrentDoctor);
                if (FastAddingMode && !(patient is UnsavedPatientViewModel))
                {
                    patient.OpenLastAppointment();
                }
                IsWordsEditing = false;
            }
            CurrentScreen = patient;
        }

        private void OnLoggedIn(object sender, LoggedEventArgs e)
        {
            IsLoginActive = false;
            IsPatientsVisible = true;
        }
    }
}