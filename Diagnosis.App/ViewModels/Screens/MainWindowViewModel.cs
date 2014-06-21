﻿using Diagnosis.App.Messaging;
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
        private ICommand _openSearchTester;
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

        #endregion Fields

        private NavigationService nav;
        private PatientViewer viewer;
        /// <summary>
        /// Установить флаг перед переходом к странице, на которой должна пустая история навигации.
        /// </summary>
        private bool clearNavOnNavigated;
        PatientsProducer patProducer = new PatientsProducer(new PatientRepository());


        #region Screen Opened flags

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

        #region Screen ViewModels

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
                return _patientsAside ?? (_patientsAside = new PatientsAsideViewModel(patProducer));
            }
        }

        public PatientsListViewModel Patients
        {
            get
            {
                return _patients ?? (_patients = new PatientsListViewModel(patProducer));
            }
        }

        public SearchViewModel Search
        {
            get { return _search ?? (_search = new SearchViewModel(patProducer)); }
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
                                              clearNavOnNavigated = true;
                                              nav.Navigate(Login);
                                          }, () => !LoginOpened));
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
                                              nav.Navigate(EntityProducers.WordsProducer);
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
                            nav.Navigate(Patients);
                        }, () => !PatientsOpened));
            }
        }

        public ICommand OpenSearchTesterCommand
        {
            get
            {
                return _openSearchTester
                   ?? (_openSearchTester = new RelayCommand(() =>
                   {
                       nav.Navigate(new SearchTester());
                   }, () => !SearchTesterOpened));
            }
        }

        public ICommand AddPatientCommand
        {
            get
            {
                return patProducer.AddPatientCommand;
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
                                              var settingsVM = new SettingsViewModel(Login.DoctorsProducer.CurrentDoctor);
                                              this.Send((int)EventID.OpenSettings, new SettingsParams(settingsVM).Params);
                                          }));
            }
        }

        #endregion Commands

        public MainWindowViewModel(NavigationService nav)
        {
            this.nav = nav;
            this.nav.Navigated += nav_Navigated;

            Login = new LoginViewModel(EntityProducers.DoctorsProducer);
            Login.LoggedIn += OnLoggedIn;

            this.Subscribe((int)EventID.PatientAdded, (e) =>
            {
                OpenPatientInViewer(e);
            });
            this.Subscribe((int)EventID.PatientCreated, (e) =>
            {
                var pat = e.GetValue<PatientViewModel>(Messages.Patient);
                viewer.OpenPatient(pat, !pat.CanAddFirstHr);
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
                }
            });
            this.Subscribe((int)EventID.OpenHealthRecord, (e) =>
            {
                var hr = e.GetValue<HealthRecord>(Messages.HealthRecord);
                var patVM = patProducer.GetByModel(hr.Appointment.Course.Patient);
                viewer.OpenPatient(patVM);
                viewer.OpenHr(hr);
            });

            //nav.Navigate(Login);
            nav.Navigate(Patients);
            Patients.SelectLastPatient();
            CreateViewer(EntityProducers.DoctorsProducer.CurrentDoctor);
        }

        private void nav_Navigated(object sender, NavigationEventArgs e)
        {
            if (clearNavOnNavigated)
            {
                // очищаем историю переходов
                while (nav.CanGoBack)
                {
                    nav.RemoveBackEntry();
                }
                clearNavOnNavigated = false;
            }

            // устанавливаем флаги экранов
            if (e.Content is PatientViewModel)
            {
                viewer.OpenPatient(e.Content as PatientViewModel);

                OnePatientOpened = true;
            }
            else if (e.Content is PatientsListViewModel)
            {
                PatientsOpened = true;
            }
            else if (e.Content is LoginViewModel)
            {
                LoginOpened = true;
            }
            else if (e.Content is WordsProducer)
            {
                WordsOpened = true;
            }
            else if (e.Content is SearchTester)
            {
                SearchTesterOpened = true;
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
            nav.Navigate(Patients);

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