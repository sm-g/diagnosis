using Diagnosis.App.Messaging;
using Diagnosis.Models;
using EventAggregator;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Diagnosis.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private bool _loginActive;
        private LoginViewModel _loginVM;
        private ICommand _logout;
        private ICommand _editSymptomsDirectory;
        private RelayCommand _openSettings;
        private bool _isPatientsVisible;
        private bool _patientOpened;
        private bool _isWordsEditing;
        private bool _fastAddingMode;
        private bool _searchTesterState;
        private SearchViewModel _search;
        private bool _searchState;
        private ViewModelBase _currentScreen;

        private NavigationService nav;
        private PatientViewer viewer;
        private bool clearNavOnNavigated;

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
                        clearNavOnNavigated = true;
                        nav.Navigate(Login);

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
                        nav.Navigate(EntityManagers.WordsManager);

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
                        nav.Navigate(new SearchTester());

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
                    else
                    {
                        viewer.ClosePatient();
                    }

                    OnPropertyChanged(() => PatientOpened);
                }
            }
        }

        #endregion CurrentScreen

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
                return viewer != null ? viewer.FastAddingMode : false;
            }
            set
            {
                if (viewer.FastAddingMode != value)
                {
                    viewer.FastAddingMode = value;
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
                                          () => nav.Content != Login));
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

        public MainWindowViewModel(NavigationService nav)
        {
            this.nav = nav;
            this.nav.Navigated += nav_Navigated;

            Login = new LoginViewModel(EntityManagers.DoctorsManager);
            Login.LoggedIn += OnLoggedIn;

            this.Subscribe((int)EventID.PatientAdded, (e) =>
            {
                OpenPatientInViewer(e);
            });
            this.Subscribe((int)EventID.PatientCreated, (e) =>
            {
                OpenPatientInViewer(e);
            });
            this.Subscribe((int)EventID.OpenPatient, (e) =>
            {
                OpenPatientInViewer(e);
            });

            this.Subscribe((int)EventID.OpenedPatientChanged, (e) =>
            {
                var pat = e.GetValue<PatientViewModel>(Messages.Patient);
                if (pat != null && nav.Content != pat) //  nav.Content == pat when navigate by history
                {
                    nav.Navigate(pat);
                    PatientOpened = true;
                }
            });
            this.Subscribe((int)EventID.OpenHealthRecord, (e) =>
            {
                var hr = e.GetValue<HealthRecord>(Messages.HealthRecord);
                var patVM = EntityManagers.PatientsManager.GetByModel(hr.Appointment.Course.Patient);
                viewer.OpenPatient(patVM);
                viewer.OpenHr(hr);
            });

            //LoginOpened = true;
            CreateViewer(EntityManagers.DoctorsManager.CurrentDoctor);
        }

        private void nav_Navigated(object sender, NavigationEventArgs e)
        {
            if (clearNavOnNavigated)
            {
                while (nav.CanGoBack)
                {
                    nav.RemoveBackEntry();
                }
                clearNavOnNavigated = false;
            }
            if (e.Content is PatientViewModel)
            {
                viewer.OpenPatient(e.Content as PatientViewModel);
            }
        }

        private void OpenPatientInViewer(EventMessage e)
        {
            var pat = e.GetValue<PatientViewModel>(Messages.Patient);
            viewer.OpenPatient(pat);
        }

        private void OnLoggedIn(object sender, LoggedEventArgs e)
        {
            IsPatientsVisible = true;
            clearNavOnNavigated = true;
            CreateViewer(e.Doctor);
        }

        private PatientViewer CreateViewer(DoctorViewModel doctor)
        {
            viewer = new PatientViewer(doctor);
            viewer.OpenLastPatient();
            return viewer;
        }
    }
}