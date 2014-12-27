using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Diagnosis.ViewModels.Screens
{
    public enum Screens
    {
        Login, Doctors, Patients, Words, Card
    }

    public class ScreenSwitcher : ViewModelBase
    {
        private List<Screens> history = new List<Screens>();
        private Screens _curScreen;
        private ScreenBase _curView;

        public ScreenSwitcher()
        {
            // диалоги

            this.Subscribe(Events.OpenSettings, (e) =>
            {
                IDialog vm;
                var user = e.GetValue<IUser>(MessageKeys.User);
                if (user is Doctor)
                    vm = new SettingsViewModel(user as Doctor);
                else
                    vm = null; // настройка админа
                this.Send(Events.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });

            this.Subscribe(Events.EditDoctor, (e) =>
            {
                var doc = e.GetValue<Doctor>(MessageKeys.Doctor);
                IDialog vm;
                if (doc != null)
                    vm = new DoctorEditorViewModel(doc);
                else
                    vm = new DoctorEditorViewModel();
                this.Send(Events.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });

            this.Subscribe(Events.EditPatient, (e) =>
            {
                var pat = e.GetValue<Patient>(MessageKeys.Patient);
                IDialog vm;
                if (pat != null)
                    vm = new PatientEditorViewModel(pat);
                else
                    vm = new PatientEditorViewModel();
                this.Send(Events.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });

            this.Subscribe(Events.EditWord, (e) =>
            {
                var w = e.GetValue<Word>(MessageKeys.Word);
                IDialog vm = new WordEditorViewModel(w);
                this.Send(Events.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });

            this.Subscribe(Events.EditHolder, (e) =>
            {
                IDialog vm;
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
                this.Send(Events.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });

            this.Subscribe(Events.CreatePatient, (e) =>
            {
                var vm = new PatientEditorViewModel();
                this.Send(Events.OpenDialog, vm.AsParams(MessageKeys.Dialog));
            });

            // экраны

            AuthorityController.LoggedIn += (s, e) =>
            {
                if (e.user is Admin)
                    OpenScreen(Screens.Doctors);
                else if (e.user is Doctor)
                    OpenScreen(Screens.Patients);

            };
            AuthorityController.LoggedOut += (s, e) =>
            {
                OpenScreen(Screens.Login);

            };
            // карточка

            this.Subscribe(Events.OpenPatient, (e) =>
            {
                // открываем экран карточки и последний осмотр пациента
                var pat = e.GetValue<Patient>(MessageKeys.Patient);

                OpenScreen(Screens.Card, pat);
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

            this.Subscribe(Events.OpenHealthRecords, (e) =>
            {
                var hrs = e.GetValue<IEnumerable<HealthRecord>>(MessageKeys.HealthRecords);
                if (hrs != null && hrs.Count() > 0)
                {
                    OpenScreen(Screens.Card, hrs);
                }
            });

            this.Subscribe(Events.EditHealthRecord, (e) =>
            {
                // открываем экран карточки, открываем запись и переключаем редактор
                var hr = e.GetValue<HealthRecord>(MessageKeys.HealthRecord);

                OpenScreen(Screens.Card, hr);
                (CurrentView as CardViewModel).ToogleHrEditor();
            });

            this.Subscribe(Events.OpenHolder, (e) =>
            {
                var holder = e.GetValue<IHrsHolder>(MessageKeys.Holder);
                OpenScreen(Screens.Card, holder);
            });

            // closing screen

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
            if (!AuthorityController.CurrentUserCanOpen(screen))
                throw new InvalidOperationException(string.Format("{0} не может открывать {1}", AuthorityController.CurrentUser, screen));

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

                    case Screens.Doctors:
                        CurrentView = new DoctorsListViewModel();
                        break;

                    case Screens.Patients:
                        CurrentView = new PatientsListViewModel();
                        break;

                    case Screens.Words:
                        CurrentView = new WordsListViewModel();
                        break;

                    case Screens.Card:
                        if (parameter != null)
                        {
                            var cardVm = new CardViewModel(false);
                            cardVm.LastItemRemoved += (s, e) =>
                            {
                                OpenScreen(Screens.Patients);
                            };
                            cardVm.Open(parameter, true);
                            CurrentView = cardVm;
                        }
                        else
                            throw new ArgumentNullException("parameter"); // что открывать в карте?
                        break;

                    default:
                        break;
                }
            }
            else
            {
                if (screen == Screens.Card)
                    (CurrentView as CardViewModel).Open(parameter);
            }
        }
    }
}