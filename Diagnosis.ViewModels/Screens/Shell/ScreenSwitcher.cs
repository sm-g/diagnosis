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

        public ScreenSwitcher()
        {
            this.Subscribe(Events.AddPatient, (e) =>
            {
                // открываем экран редактора пациента, в нём новый пациент
                OpenScreen(Screens.PatientEditor, true);
            });

            this.Subscribe(Events.OpenPatient, (e) =>
            {
                // открываем экран карточки и пациента

                var patient = e.GetValue<Patient>(MessageKeys.Patient);

                OpenScreen(Screens.Card);
                CardViewModel.viewer.OpenPatient(patient);
            });

            this.Subscribe(Events.FirstHr, (e) =>
            {
                // открываем экран карточки и пациента
                var pat = e.GetValue<Patient>(MessageKeys.Patient);
                OpenScreen(Screens.Card);

                CardViewModel.viewer.OpenPatient(pat, true);

            });

            this.Subscribe(Events.EditPatient, (e) =>
            {
                var pat = e.GetValue<Patient>(MessageKeys.Patient);

                OpenScreen(Screens.PatientEditor, false, pat);
            });

            this.Subscribe(Events.LeavePatientEditor, (e) =>
            {
                // возвращаемся к спсику пациентом или к карточке пациента
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

            this.Subscribe(Events.OpenHealthRecord, (e) =>
            {
                var hr = e.GetValue<HealthRecord>(MessageKeys.HealthRecord);
                OpenScreen(Screens.Card);
                CardViewModel.viewer.OpenHr(hr);
            });

            this.Subscribe(Events.EditHealthRecord, (e) =>
            {
                // открываем экран карточки, открываем запись, загружаем в редактор запись и показываем редактор
                var hr = e.GetValue<HealthRecord>(MessageKeys.HealthRecord);

                OpenScreen(Screens.Card);
                CardViewModel.viewer.OpenHr(hr);
                (CurrentView as CardViewModel).EditHr();
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
        /// <summary>
        /// Открывает экран.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="replace">Открывать ли экран заново, если совпадает с текущим экраном.</param>
        public void OpenScreen(Screens screen, bool replace = false, object parameter = null)
        {
            var updateCurView = replace || Screen != screen; // не обновляем, если экран тот же и не надо заменять

            Screen = screen;

            if (updateCurView)
            {
                if (_curView != null)
                    _curView.Dispose();

                switch (screen)
                {
                    case Screens.Login:
                        CurrentView = new LoginViewModel();
                        break;

                    case Screens.Patients:
                        CurrentView = new PatientsListViewModel();
                        break;

                    case Screens.Words:
                        CurrentView = new WordsListViewModel();
                        break;

                    case Screens.Card:
                        CurrentView = new CardViewModel();
                        break;

                    case Screens.PatientEditor:
                        if (parameter != null)
                            CurrentView = new PatientEditorViewModel(parameter as Patient);
                        else
                            // новый пациент в редакторе
                            CurrentView = new PatientEditorViewModel();

                        break;

                    default:
                        break;
                }
            }
            if (screen != Screens.Card)
            {
                CardViewModel.viewer.ClosePatient();
            }
        }

    }
}
