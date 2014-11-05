using System;
using System.Windows.Navigation;
using EventAggregator;
using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
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
        private ScreenBase _curView;

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

            this.Subscribe(Events.ShowPatient, (e) =>
            {
                // открываем карточку или редактор пациента
                var pat = e.GetValue<Patient>(MessageKeys.Patient);

                if (Screen == Screens.PatientEditor)
                {
                    OpenScreen(Screens.PatientEditor, pat, true);
                }
                else
                {
                    OpenScreen(Screens.Card, pat, true);
                }
            });

            this.Subscribe(Events.OpenCourse, (e) =>
            {
                var course = e.GetValue<Course>(MessageKeys.Course);
                OpenScreen(Screens.Card, course);
            });

            this.Subscribe(Events.OpenAppointment, (e) =>
            {
                var app = e.GetValue<Appointment>(MessageKeys.Appointment);
                OpenScreen(Screens.Card, app);
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
                // открываем экран карточки, открываем запись и переключаем редактор
                var hr = e.GetValue<HealthRecord>(MessageKeys.HealthRecord);

                OpenScreen(Screens.Card, hr);
                (CurrentView as CardTreeViewModel).ToogleHrEditor();
            });

            this.Subscribe(Events.OpenHolder, (e) =>
            {
                var holder = e.GetValue<IHrsHolder>(MessageKeys.Holder);
                OpenScreen(Screens.Card, holder);
            });

            this.Subscribe(Events.Shutdown, (e) =>
            {
                CurrentView.Dispose();
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

        public ScreenBase CurrentView
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
                        if (parameter != null)
                            CurrentView = new CardTreeViewModel(parameter);
                        else
                            throw new ArgumentNullException("parameter"); // что открывать в карте?
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
                    (CurrentView as CardTreeViewModel).Open(parameter);
            }
        }

    }
}
