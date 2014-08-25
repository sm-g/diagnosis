using Diagnosis.Core;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Collections.Generic;
using Diagnosis.App.Messaging;

namespace Diagnosis.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private ICommand _logout;
        private ICommand _openWords;
        private ICommand _openPatients;
        private ICommand _openSettings;
        private bool _patientsAsideOpened;
        private bool _patientsAsideVisible;
        private bool _onePatientOpened;
        private bool _loginOpened;
        private bool _patientsOpened;
        private bool _wordsOpened;
        private bool _searchAsideOpened;
        private SearchViewModel _search;
        private PatientsAsideViewModel _patientsAside;
        private PatientsListViewModel _patients;
        private LoginViewModel _login;
        WordsListViewModel _words;

        #endregion Fields

        [Flags]
        private enum Screens
        {
            Login, Patients, Words, Patient
        }
        private NavigationService nav;
        private PatientViewer viewer;

        /// <summary>
        /// Установить флаг перед переходом к странице, на которой должна пустая история навигации.
        /// </summary>
        private bool clearNavOnNavigated;

        private PatientsProducer patProducer = new PatientsProducer(new PatientRepository());

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

                    this.Send((int)EventID.WordsEditingModeChanged, new DirectoryEditingModeChangedParams(value).Params);
                    OnPropertyChanged(() => WordsOpened);
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
                    _patientsOpened = value;
                    OnPropertyChanged(() => PatientsOpened);
                }
            }
        }

        #endregion Screen Opened flags

        #region Flags

        DateTime[] asideOpenedChangedAt = new DateTime[2];

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
                }
                if (!value)
                {
                    PatientsAsideOpened = false;
                }
            }
        }

        public bool PatientsAsideOpened
        {
            get
            {
                return _patientsAsideOpened;
            }
            set
            {
                if (_patientsAsideOpened != value && NotReOpenedFix(0))
                {
                    asideOpenedChangedAt[0] = DateTime.UtcNow;

                    _patientsAsideOpened = value;
                    OnPropertyChanged(() => PatientsAsideOpened);
                    OnPropertyChanged(() => NoAsideOpened);
                }
            }
        }
        public bool SearchAsideOpened
        {
            get
            {
                return _searchAsideOpened;
            }
            set
            {
                if (_searchAsideOpened != value && NotReOpenedFix(1))
                {
                    asideOpenedChangedAt[1] = DateTime.UtcNow;

                    _searchAsideOpened = value;
                    OnPropertyChanged(() => SearchAsideOpened);
                    OnPropertyChanged(() => NoAsideOpened);
                }
            }
        }

        public bool NoAsideOpened
        {
            get
            {
                return !SearchAsideOpened && !PatientsAsideOpened;
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

        bool NotReOpenedFix(int asideIndex)
        {
            // Если открываем aside по togglebutton, а закрываем через меню, aside открывается повторно
            return (DateTime.UtcNow - asideOpenedChangedAt[asideIndex]).TotalMilliseconds > 100;
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

        public WordsListViewModel Words
        {
            get { return _words ?? (_words = new WordsListViewModel()); }
        }

        #endregion Screen ViewModels

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
                                              nav.Navigate(Words);
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
            this.Subscribe((int)EventID.SendToSearch, (e) =>
            {
                SearchAsideOpened = true;
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

            if (e.Content is PatientViewModel)
            {
                OpenScreen(Screens.Patient);
                // сначала открываем экран, затем пациента

                viewer.OpenPatient(e.Content as PatientViewModel);
            }
            else if (e.Content is PatientsListViewModel)
            {
                OpenScreen(Screens.Patients);
            }
            else if (e.Content is LoginViewModel)
            {
                OpenScreen(Screens.Login);
            }
            else if (e.Content is WordsListViewModel)
            {
                OpenScreen(Screens.Words);
            }
        }

        /// <summary>
        /// Устанавливает флаги экранов и элементов
        /// </summary>
        /// <param name="screen"></param>
        private void OpenScreen(Screens screen)
        {
            LoginOpened = false;
            OnePatientOpened = false;
            PatientsOpened = false;
            WordsOpened = false;

            switch (screen)
            {
                case Screens.Login: LoginOpened = true; break;
                case Screens.Patient: OnePatientOpened = true; break;
                case Screens.Patients: PatientsOpened = true; break;
                case Screens.Words: WordsOpened = true; break;
            }

            if (screen != Screens.Patient)
            {
                viewer.ClosePatient();
            }

            if ((screen & (Screens.Login | Screens.Words)) == screen)
            {
                SearchAsideOpened = false;
                PatientsAsideOpened = false;
            }

            if (screen == Screens.Patients)
            {
                PatientsAsideVisible = false;
            }
            else
            {
                PatientsAsideVisible = true;
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