using EventAggregator;
using System.Windows.Input;
using Diagnosis.App.Messaging;
using Diagnosis.Models;

namespace Diagnosis.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private bool _loginActive;
        private LoginViewModel _loginVM;
        private ICommand _logout;
        private ICommand _editDiagnosisDirectory;
        private ICommand _editSymptomsDirectory;
        private RelayCommand _openSettings;
        private bool _fastAddingMode;
        private bool _isPatientsVisible;
        bool _patientOpened;
        private bool _isWordsEditing;
        private bool _searchTesterState;
        private SearchViewModel _search;
        private bool _searchState;
        private ViewModelBase _currentScreen;
        private object _directoryExplorer;

        #endregion Fields

        #region CurrentScreen

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
                    if (!(value is PatientViewModel))
                    {
                        EntityManagers.PatientsManager.RemoveCurrent();
                    }
                    OnPropertyChanged(() => CurrentScreen);
                }
            }
        }

        public bool LoginOpened
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
                        CurrentScreen = Login;

                        WordsOpened = false;
                        SearchTesterOpened = false;
                        PatientOpened = false;
                    }

                    OnPropertyChanged(() => LoginOpened);
                }
            }
        }

        public bool WordsOpened
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
                        CurrentScreen = EntityManagers.WordsManager;

                        LoginOpened = false;
                        SearchTesterOpened = false;
                        PatientOpened = false;
                    }

                    this.Send((int)EventID.WordsEditingModeChanged, new DirectoryEditingModeChangedParams(value).Params);
                    OnPropertyChanged(() => WordsOpened);
                }
            }
        }

        public bool SearchTesterOpened
        {
            get
            {
                return _searchTesterState;
            }
            set
            {
                if (_searchTesterState != value)
                {
                    _searchTesterState = value;
                    if (value)
                    {
                        CurrentScreen = new SearchTester();

                        LoginOpened = false;
                        WordsOpened = false;
                        PatientOpened = false;
                    }

                    OnPropertyChanged(() => SearchTesterOpened);
                }
            }
        }
        public bool PatientOpened
        {
            get
            {
                return _patientOpened;
            }
            set
            {
                if (_patientOpened != value)
                {
                    _patientOpened = value;
                    if (value)
                    {
                        LoginOpened = false;
                        WordsOpened = false;
                        SearchTesterOpened = false;
                    }

                    OnPropertyChanged(() => PatientOpened);
                }
            }
        }

        #endregion

        #region Flags

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
                    OnPropertyChanged(() => NoTabVisible);
                }
            }
        }
        public bool IsSearchVisible
        {
            get
            {
                return _searchState;
            }
            set
            {
                if (_searchState != value)
                {
                    _searchState = value;
                    OnPropertyChanged(() => IsSearchVisible);
                    OnPropertyChanged(() => NoTabVisible);
                }
            }
        }

        public bool NoTabVisible
        {
            get
            {
                return !IsSearchVisible && !IsPatientsVisible;
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

        #endregion Flags

        #region ViewModels
        public LoginViewModel Login
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
                    OnPropertyChanged(() => Login);
                }
            }
        }

        public PatientsManager Patients
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
        public SearchViewModel Search
        {
            get { return _search ?? (_search = new SearchViewModel()); }
        }

        #endregion ViewModels

        #region Commands

        public ICommand LogoutCommand
        {
            get
            {
                return _logout ?? (_logout = new RelayCommand(
                                          () =>
                                          {
                                              LoginOpened = true;
                                          },
                                          () => !LoginOpened));
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
                                              WordsOpened = true;
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
        public RelayCommand OpenSettingsCommand
        {
            get
            {
                return _openSettings
                    ?? (_openSettings = new RelayCommand(
                                          () =>
                                          {
                                              var settingsVM = new SettingsViewModel(Login.DoctorsManager.CurrentDoctor);
                                              this.Send((int)EventID.OpenSettings, new SettingsParams(settingsVM).Params);
                                          }));
            }
        }

        #endregion Commands

        public MainWindowViewModel()
        {
            this.Subscribe((int)EventID.CurrentPatientChanged, (e) =>
            {
                var patient = e.GetValue<PatientViewModel>(Messages.Patient);
                ShowPatient(patient);
            });
            this.Subscribe((int)EventID.OpenHealthRecord, (e) =>
            {
                var hr = e.GetValue<HealthRecord>(Messages.HealthRecord);
                var patVM = EntityManagers.PatientsManager.GetByModel(hr.Appointment.Course.Patient);
                EntityManagers.PatientsManager.CurrentPatient = patVM;
                patVM.CoursesManager.OpenHr(hr);
            });

            Login = new LoginViewModel(EntityManagers.DoctorsManager);
            Login.LoggedIn += OnLoggedIn;
        }

        private void ShowPatient(PatientViewModel patient)
        {
            if (patient != null)
            {
                patient.SetDoctorVM(Login.DoctorsManager.CurrentDoctor);
                if (FastAddingMode && !(patient is UnsavedPatientViewModel))
                {
                    EntityManagers.PatientsManager.OpenLastAppointment(patient);
                }
                CurrentScreen = patient;
            }
            PatientOpened = true;
        }

        private void OnLoggedIn(object sender, LoggedEventArgs e)
        {
            IsPatientsVisible = true;
            EntityManagers.PatientsManager.SetCurrentToLast();
        }
    }
}