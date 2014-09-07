using System;
using System.Windows.Navigation;
using EventAggregator;
using Diagnosis.Core;
using Diagnosis.Models;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.ViewModels
{
    [Flags]
    public enum Screens
    {
        Login, Patients, Words, Card, PatientEditor
    }

    public class ScreenSwitcher : ViewModelBase
    {
        List<Screens> history = new List<Screens>();
        private Screens _curScreen;
        private SessionVMBase _curView;
        private LoginViewModel _login;
        private PatientsListViewModel _patients;
        private WordsListViewModel _words;

        public ScreenSwitcher()
        {
            this.Subscribe(Events.OpenPatient, (e) =>
                        {
                            // где история открытых?

                            var patient = e.GetValue<Patient>(MessageKeys.Patient);

                            if (Screen == Screens.Card)
                            {
                                CardViewModel.viewer.OpenPatient(patient);
                            }
                            else
                            {
                                OpenScreen(Screens.Card);
                                CardViewModel.viewer.OpenPatient(patient);
                            }
                        });

            this.Subscribe(Events.PatientSaved, (e) =>
                        {
                            var pat = e.GetValue<Patient>(MessageKeys.Patient);
                            for (int i = history.Count - 1; i >= 0; i--)
                            {
                                if (history[i] == Screens.Card)
                                {
                                    OpenScreen(Screens.Card);
                                    return;
                                }
                                if (history[i] == Screens.Patients)
                                {
                                    OpenScreen(Screens.Patients);
                                    return;
                                }
                            }

                        });
            this.Subscribe(Events.FirstHr, (e) =>
                        {
                            var pat = e.GetValue<Patient>(MessageKeys.Patient);
                            OpenScreen(Screens.Card);

                            CardViewModel.viewer.OpenPatient(pat, true);

                        });
            this.Subscribe(Events.AddPatient, (e) =>
                        {
                            // открываем экран редактора пациента, в нём новый пациент
                            OpenScreen(Screens.PatientEditor);
                        });
            this.Subscribe(Events.OpenHealthRecord, (e) =>
                        {
                            var hr = e.GetValue<HealthRecord>(MessageKeys.HealthRecord);
                            OpenScreen(Screens.Card);
                            CardViewModel.viewer.OpenHr(hr);
                        });
        }

        public Screens Screen
        {
            get
            {
                return _curScreen;
            }
            set
            {
                if (_curScreen != value)
                {
                    Debug.Print("Screen {0}", value);
                    history.Add(value);
                    _curScreen = value;
                    OnPropertyChanged(() => Screen);
                }
            }
        }

        public SessionVMBase CurrentView
        {
            get
            {
                return _curView;
            }
            set
            {
                if (_curView != value)
                {
                    _curView = value;
                    OnPropertyChanged(() => CurrentView);
                }
            }
        }

        public LoginViewModel Login
        {
            get
            {
                return _login ?? (_login = new LoginViewModel());
            }
        }

        public PatientsListViewModel Patients
        {
            get { return _patients ?? (_patients = new PatientsListViewModel()); }
        }

        public WordsListViewModel Words
        {
            get { return _words ?? (_words = new WordsListViewModel()); }
        }

        public void OpenScreen(Screens screen)
        {
            Screen = screen;
            switch (screen)
            {
                case Diagnosis.ViewModels.Screens.Login:
                    CurrentView = new LoginViewModel();
                    break;

                case Diagnosis.ViewModels.Screens.Patients:
                    CurrentView = new PatientsListViewModel();
                    break;

                case Diagnosis.ViewModels.Screens.Words:
                    CurrentView = new WordsListViewModel();
                    break;

                case Diagnosis.ViewModels.Screens.Card:
                    CurrentView = new CardViewModel();
                    break;

                case Screens.PatientEditor:
                    CurrentView = new PatientEditorViewModel();
                    break;

                default:
                    break;
            }

            if (screen != Screens.Card)
            {
                CardViewModel.viewer.ClosePatient();
            }
        }

    }
}
