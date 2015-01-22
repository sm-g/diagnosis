using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Diagnosis.ViewModels.Screens
{
    public enum Screen
    {
        Login, Doctors, Patients, Words, Card
    }

    public class ScreenSwitcher : ViewModelBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(ScreenSwitcher));
        private List<Screen> history = new List<Screen>();
        private Screen _curScreen;
        private ScreenBaseViewModel _curView;

        public ScreenSwitcher()
        {
            // диалоги

            this.Subscribe(Event.OpenSettings, (e) =>
            {
                IDialogViewModel vm;
                var user = e.GetValue<IUser>(MessageKeys.User);
                if (user is Doctor)
                    vm = new SettingsViewModel(user as Doctor);
                else
                    vm = null; // настройка админа
                this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });

            this.Subscribe(Event.EditDoctor, (e) =>
            {
                var doc = e.GetValue<Doctor>(MessageKeys.Doctor);
                IDialogViewModel vm;
                if (doc != null)
                    vm = new DoctorEditorViewModel(doc);
                else
                    vm = new DoctorEditorViewModel();
                this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });

            this.Subscribe(Event.EditPatient, (e) =>
            {
                var pat = e.GetValue<Patient>(MessageKeys.Patient);
                IDialogViewModel vm;
                if (pat != null)
                    vm = new PatientEditorViewModel(pat);
                else
                    vm = new PatientEditorViewModel();
                this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });

            this.Subscribe(Event.EditWord, (e) =>
            {
                var w = e.GetValue<Word>(MessageKeys.Word);
                IDialogViewModel vm = new WordEditorViewModel(w);
                this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });

            this.Subscribe(Event.EditHolder, (e) =>
            {
                IDialogViewModel vm;
                var holder = e.GetValue<IHrsHolder>(MessageKeys.Holder);
                if (holder is Appointment)
                {
                    vm = new AppointmentEditorViewModel(holder as Appointment);
                }
                else if (holder is Course)
                {
                    vm = new CourseEditorViewModel(holder as Course);
                }
                else // holder is Patient
                {
                    vm = new PatientEditorViewModel(holder as Patient);
                }
                this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });

            this.Subscribe(Event.CreatePatient, (e) =>
            {
                var vm = new PatientEditorViewModel();
                this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });

            // экраны

            AuthorityController.LoggedIn += (s, e) =>
            {
                if (e.user is Admin)
                    OpenScreen(Screen.Doctors);
                else if (e.user is Doctor)
                    OpenScreen(Screen.Patients);

            };
            AuthorityController.LoggedOut += (s, e) =>
            {
                OpenScreen(Screen.Login);

            };
            // карточка

            this.Subscribe(Event.OpenPatient, (e) =>
            {
                var pat = e.GetValue<Patient>(MessageKeys.Patient);
                OpenScreen(Screen.Card, pat);
            });

            this.Subscribe(Event.OpenCourse, (e) =>
            {
                var course = e.GetValue<Course>(MessageKeys.Course);
                OpenScreen(Screen.Card, course);
            });

            this.Subscribe(Event.OpenAppointment, (e) =>
            {
                var app = e.GetValue<Appointment>(MessageKeys.Appointment);
                OpenScreen(Screen.Card, app);
            });

            this.Subscribe(Event.OpenHealthRecords, (e) =>
            {
                var hrs = e.GetValue<IEnumerable<HealthRecord>>(MessageKeys.HealthRecords);
                if (hrs != null && hrs.Count() > 0)
                {
                    OpenScreen(Screen.Card, hrs);
                }
            });

            this.Subscribe(Event.EditHealthRecord, (e) =>
            {
                // открываем экран карточки, открываем запись
                // переходим в редактор или переключаем видимость редактора
                var hr = e.GetValue<HealthRecord>(MessageKeys.HealthRecord);
                var goToEditor = e.GetValue<bool>(MessageKeys.Boolean);

                if (goToEditor)
                    logger.DebugFormat("goto editor for {0}", this);
                else
                    logger.DebugFormat("toggle editor for {0}", this);

                OpenScreen(Screen.Card, hr);

                var card = (CurrentView as CardViewModel);
                if (goToEditor)
                    card.FocusHrEditor(hr);
                else
                    card.ToogleHrEditor();
            });

            this.Subscribe(Event.OpenHolder, (e) =>
            {
                var holder = e.GetValue<IHrsHolder>(MessageKeys.Holder);
                OpenScreen(Screen.Card, holder);
            });

            // closing screen

            this.Subscribe(Event.Shutdown, (e) =>
            {
                if (CurrentView != null)
                {
                    CurrentView.Dispose();

                }
            });
        }

        public Screen Screen
        {
            get
            {
                return _curScreen;
            }
            set
            {
                if (_curScreen != value)
                {
                    history.Add(value);
                    logger.DebugFormat("screen {0} -> {1}", _curScreen, value);
                    _curScreen = value;
                    OnPropertyChanged(() => Screen);
                }
            }
        }

        public ScreenBaseViewModel CurrentView
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
        public void OpenScreen(Screen screen, object parameter = null, bool replace = false)
        {
            if (!AuthorityController.CurrentUserCanOpen(screen))
                throw new InvalidOperationException(string.Format("{0} не может открывать {1}", AuthorityController.CurrentUser, screen));

            var updateCurView = replace || Screen != screen; // не обновляем, если экран тот же и не надо заменять
            Screen = screen;

            if (updateCurView)
            {
                if (_curView != null)
                    _curView.Dispose();

                switch (screen)
                {
                    case Screen.Login:
                        CurrentView = new LoginViewModel();
                        break;

                    case Screen.Doctors:
                        CurrentView = new DoctorsListViewModel();
                        break;

                    case Screen.Patients:
                        CurrentView = new PatientsListViewModel();
                        break;

                    case Screen.Words:
                        CurrentView = new WordsListViewModel();
                        break;

                    case Screen.Card:
                        var cardVm = new CardViewModel(false);
                        cardVm.LastItemRemoved += (s, e) =>
                        {
                            OpenScreen(Screen.Patients);
                        };
                        cardVm.Open(parameter, lastAppOrCourse: true);
                        CurrentView = cardVm;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                if (screen == Screen.Card)
                    (CurrentView as CardViewModel).Open(parameter);
            }
        }
    }
}