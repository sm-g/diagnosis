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
                OpenScreen(Screens.PatientEditor, replace: true);
            });

            this.Subscribe(Events.OpenPatient, (e) =>
            {
                // открываем экран карточки и пациента

                var pat = e.GetValue<Patient>(MessageKeys.Patient);

                OpenScreen(Screens.Card, pat);
            });

            this.Subscribe(Events.FirstHr, (e) =>
            {
                // открываем экран карточки и пациента
                var pat = e.GetValue<Patient>(MessageKeys.Patient);
                OpenScreen(Screens.Card, pat);
                //
            });

            this.Subscribe(Events.EditPatient, (e) =>
            {
                var pat = e.GetValue<Patient>(MessageKeys.Patient);

                OpenScreen(Screens.PatientEditor, pat);
            });

            this.Subscribe(Events.LeavePatientEditor, (e) =>
            {
                // возвращаемся к предыдущему экрану
                var pat = e.GetValue<Patient>(MessageKeys.Patient);

                for (int i = history.Count - 1; i >= 0; i--)
                {
                    if (history[i] != Screens.PatientEditor)
                    {
                        OpenScreen(history[i], pat);
                        return;
                    }
                }

            });

            this.Subscribe(Events.OpenHealthRecord, (e) =>
            {
                var hr = e.GetValue<HealthRecord>(MessageKeys.HealthRecord);
                OpenScreen(Screens.Card, hr);
            });

            this.Subscribe(Events.EditHealthRecord, (e) =>
            {
                // открываем экран карточки, открываем запись, загружаем в редактор запись и показываем редактор
                var hr = e.GetValue<HealthRecord>(MessageKeys.HealthRecord);

                OpenScreen(Screens.Card, hr);
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
        public void OpenScreen(Screens screen, object parameter = null, bool replace = false)
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
                        if (parameter is Patient)
                            CurrentView = new CardViewModel(parameter as Patient);
                        else if (parameter is HealthRecord)
                            CurrentView = new CardViewModel(parameter as HealthRecord);

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
            else
            {
                if (screen == Screens.Card)
                    if (parameter is Patient)
                        (CurrentView as CardViewModel).OpenPatient(parameter as Patient);
                    else if (parameter is HealthRecord)
                        (CurrentView as CardViewModel).OpenHr(parameter as HealthRecord);
            }
        }

    }
}
