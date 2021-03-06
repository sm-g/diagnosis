﻿using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public enum Screen
    {
        Login, Doctors, Patients, Words, Card, Sync, Vocabularies, Criteria
    }

    public class ScreenSwitcher : NotifyPropertyChangedBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(ScreenSwitcher));
        private List<Screen> history = new List<Screen>();
        private Screen _curScreen;
        private ScreenBaseViewModel _curView;
        AuthorityController ac;

        public ScreenSwitcher(AuthorityController ac)
        {
            this.ac = ac;

            // диалоги

            SubscribeForSendOpenDialog();

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

            SubscribeForOpenInCard();
            SubscribeForOpenInCriteria();

            // closing screen

            this.Subscribe(Event.Shutdown, (e) =>
            {
                if (CurrentView != null)
                {
                    CurrentView.Dispose();
                }
            });
            this.Subscribe(Event.NewSession, (e) =>
            {
                AuthorityController.Default.LogOut();
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
        /// Показывать поиск на текущем экране.
        /// </summary>
        public bool WithSearch
        {
            get { return ac.CurrentDoctor != null; }
        }

        /// <summary>
        /// Показывать меню на текущем экране.
        /// </summary>
        public bool WithMenuBar
        {
            get { return Screen != Screens.Screen.Login; }
        }

        /// <summary>
        /// Открывает экран.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="replace">Открывать ли экран заново, если совпадает с текущим экраном.</param>
        public void OpenScreen(Screen screen, object parameter = null, bool replace = false)
        {
            if (!ac.CurrentUserCanOpen(screen))
                throw new InvalidOperationException(string.Format("{0} не может открывать {1}", ac.CurrentUser, screen));

            var updateCurView = replace || Screen != screen;
            Screen = screen;

            if (updateCurView)
            {
                if (_curView != null)
                    _curView.Dispose();

                switch (screen)
                {
                    case Screen.Login:
                        var vm = new LoginViewModel();
                        if (!vm.LoggedIn) // автовход - не открываем логин
                            CurrentView = vm;
                        break;

                    case Screen.Doctors:
                        CurrentView = new DoctorsListViewModel();
                        break;

                    case Screen.Sync:
                        CurrentView = new SyncViewModel(Constants.ServerConnectionInfo);
                        break;

                    case Screen.Vocabularies:
                        CurrentView = new VocabularySyncViewModel(Constants.ServerConnectionInfo);
                        break;

                    case Screen.Patients:
                        CurrentView = new PatientsListViewModel();
                        break;

                    case Screen.Words:
                        CurrentView = new WordsListViewModel();
                        break;

                    case Screen.Criteria:
                        var crVm = new CriteriaViewModel();
                        var crit = parameter as ICrit;
                        if (crit != null)
                        {
                            crVm.Open(crit);
                        }
                        CurrentView = crVm;
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
            else // только открываем сущность на экране
            {
                if (screen == Screen.Card)
                    (CurrentView as CardViewModel).Open(parameter);
                else if (screen == Screen.Criteria)
                {
                    var crit = parameter as ICrit;
                    if (crit != null)
                    {
                        (CurrentView as CriteriaViewModel).Open(crit);
                    }
                }
            }
        }

        private void SubscribeForOpenInCriteria()
        {
            this.Subscribe(Event.OpenCrit, (e) =>
            {
                var crit = e.GetValue<ICrit>(MessageKeys.Crit);
                OpenScreen(Screen.Criteria, crit);
            });

            this.Subscribe(Event.EditCrit, (e) =>
            {
                var crit = e.GetValue<ICrit>(MessageKeys.Crit);
                OpenScreen(Screen.Criteria, crit);
            });
        }

        private void SubscribeForOpenInCard()
        {
            this.Subscribe(Event.OpenHolder, (e) =>
            {
                var holder = e.GetValue<IHrsHolder>(MessageKeys.Holder);
                OpenScreen(Screen.Card, holder);
            });

            this.Subscribe(Event.OpenHealthRecords, (e) =>
            {
                var hrs = e.GetValue<IEnumerable<HealthRecord>>(MessageKeys.HealthRecords);
                if (hrs != null && hrs.Any())
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

                OpenScreen(Screen.Card, hr);

                var card = (CurrentView as CardViewModel);
                if (goToEditor)
                    card.StartEditHr(hr);
                else
                    card.ToogleHrEditor();
            });
        }

        private void SubscribeForSendOpenDialog()
        {
            this.Subscribe(Event.OpenSettings, (e) =>
            {
                IDialogViewModel vm;
                var user = e.GetValue<IUser>(MessageKeys.User);
                if (user is Doctor)
                    vm = new SettingsViewModel(user as Doctor);
                else
                    vm = new AdminSettingsViewModel(user as Admin);
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
        }
    }
}