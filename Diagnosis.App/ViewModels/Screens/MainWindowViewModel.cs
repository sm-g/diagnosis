using Diagnosis.App.Messaging;
using Diagnosis.Models;
using Diagnosis.Data.Repositories;
using EventAggregator;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Diagnosis.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private ICommand _logout;
        private ICommand _openWords;
        private ICommand _openPatients;
        private ICommand _openSettings;
        private bool _patientsAsideVisible;
        private bool _onePatientOpened;
        private bool _loginOpened;
        private bool _patientsOpened;
        private bool _wordsOpened;
        private bool _searchTesterOpened;
        private bool _searchAsideVisible;
        private SearchViewModel _search;
        private PatientsAsideViewModel _patientsAside;
        private PatientsListViewModel _patients;
        private LoginViewModel _login;

        private ViewModelBase _currentScreen;

        #endregion Fields

        private NavigationService nav;
        private PatientViewer viewer;
        /// <summary>
        /// Установить флаг перед переходом к странице, на которой должна пустая история навигации.
        /// </summary>
        private bool clearNavOnNavigated;
        PatientsManager patManager = new PatientsManager(new PatientRepository());


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
                return _loginOpened;
            }
            set
            {
                if (_loginOpened != value)
                {
                    _loginOpened = value;
                    if (value)
                    {
                        clearNavOnNavigated = true;
                        nav.Navigate(Login);

                        WordsOpened = false;
                        PatientsOpened = false;
                        SearchTesterOpened = false;
                        OnePatientOpened = false;

                        SearchAsideVisible = false;
                        PatientsAsideVisible = false;
                    }

                    OnPropertyChanged(() => LoginOpened);
                }
            }
        }

        public bool WordsOpened
        {
            get
            {
                return _wordsOpened;
            }
            set
            {
                if (_wordsOpened != value)
                {
                    _wordsOpened = value;

                    if (value)
                    {
                        nav.Navigate(EntityManagers.WordsManager);

                        LoginOpened = false;
                        SearchTesterOpened = false;
                        PatientsOpened = false;
                        OnePatientOpened = false;

                        SearchAsideVisible = false;
                        PatientsAsideVisible = false;
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
                return _searchTesterOpened;
            }
            set
            {
                if (_searchTesterOpened != value)
                {
                    _searchTesterOpened = value;
                    if (value)
                    {
                        nav.Navigate(new SearchTester());

                        LoginOpened = false;
                        WordsOpened = false;
                        PatientsOpened = false;
                        OnePatientOpened = false;

                        SearchAsideVisible = false;
                        PatientsAsideVisible = false;
                    }

                    OnPropertyChanged(() => SearchTesterOpened);
                }
            }
        }

        public bool OnePatientOpened
        {
            get
            {
                return _onePatientOpened;
            }
            set
            {
                if (_onePatientOpened != value)
                {
                    _onePatientOpened = value;
                    if (value)
                    {
                        LoginOpened = false;
                        WordsOpened = false;
                        SearchTesterOpened = false;
                        PatientsOpened = false;
                    }
                    else
                    {
                        viewer.ClosePatient();
                    }

                    OnPropertyChanged(() => OnePatientOpened);
                }
            }
        }

        public bool PatientsOpened
        {
            get
            {
                return _patientsOpened;
            }
            set
            {
                if (_patientsOpened != value)
                {
                    if (value)
                    {
                        nav.Navigate(Patients);

                        OnePatientOpened = false;
                        LoginOpened = false;
                        WordsOpened = false;
                        SearchTesterOpened = false;

                        PatientsAsideVisible = false;
                    }

                    _patientsOpened = value;
                    OnPropertyChanged(() => PatientsOpened);
                }
            }
        }

        #endregion CurrentScreen

        #region Flags

        public bool PatientsAsideVisible
        {
            get
            {
                return _patientsAsideVisible;
            }
            set
            {
                if (_patientsAsideVisible != value)
                {
                    _patientsAsideVisible = value;
                    OnPropertyChanged(() => PatientsAsideVisible);
                    OnPropertyChanged(() => NoAsideVisible);
                }
            }
        }

        public bool SearchAsideVisible
        {
            get
            {
                return _searchAsideVisible;
            }
            set
            {
                if (_searchAsideVisible != value)
                {
                    _searchAsideVisible = value;
                    OnPropertyChanged(() => SearchAsideVisible);
                    OnPropertyChanged(() => NoAsideVisible);
                }
            }
        }

        public bool NoAsideVisible
        {
            get
            {
                return !SearchAsideVisible && !PatientsAsideVisible;
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
                return _login;
            }
            set
            {
                if (_login != value)
                {
                    _login = value;
                    OnPropertyChanged(() => Login);
                }
            }
        }

        public PatientsAsideViewModel PatientsAside
        {
            get
            {
                return _patientsAside ?? (_patientsAside = new PatientsAsideViewModel(patManager));
            }
        }

        public PatientsListViewModel Patients
        {
            get
            {
                return _patients ?? (_patients = new PatientsListViewModel(patManager));
            }
        }

        public SearchViewModel Search
        {
            get { return _search ?? (_search = new SearchViewModel(patManager)); }
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
                                          }, () => nav.Content != Login));
            }
        }

        public ICommand OpenWordsCommand
        {
            get
            {
                return _openWords
                    ?? (_openWords = new RelayCommand(
                                          () =>
                                          {
                                              WordsOpened = true;
                                          }, () => !WordsOpened));
            }
        }

        public ICommand OpenPatientsCommand
        {
            get
            {
                return _openPatients
                   ?? (_openPatients = new RelayCommand(() =>
                        {
                            PatientsOpened = true;
                        }, () => !PatientsOpened));
            }
        }

        public ICommand AddPatientCommand
        {
            get
            {
                return patManager.AddPatientCommand;
            }
        }

        public ICommand OpenSettingsCommand
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
                var patVM = e.GetValue<PatientViewModel>(Messages.Patient);
                viewer.OpenLastAppointment(patVM);
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
                    OnePatientOpened = true;
                }
            });
            this.Subscribe((int)EventID.OpenHealthRecord, (e) =>
            {
                var hr = e.GetValue<HealthRecord>(Messages.HealthRecord);
                var patVM = patManager.GetByModel(hr.Appointment.Course.Patient);
                viewer.OpenPatient(patVM);
                viewer.OpenHr(hr);
            });

            //LoginOpened = true;
            PatientsOpened = true;
            Patients.SelectLastPatient();
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
            clearNavOnNavigated = true;
            PatientsOpened = true;
            CreateViewer(e.Doctor);
            Patients.SelectLastPatient();
        }

        private PatientViewer CreateViewer(DoctorViewModel doctor)
        {
            viewer = new PatientViewer(doctor);
            return viewer;
        }
    }
}