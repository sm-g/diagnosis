using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace Diagnosis.ViewModels.Screens
{
    public class MenuBarViewModel : ViewModelBase
    {
        private ScreenSwitcher switcher;
        private bool _visible;
        HelpViewModel help;

        public MenuBarViewModel(ScreenSwitcher switcher, SearchViewModel sPanel)
        {
            this.switcher = switcher;

            SearchPanel = sPanel;
            AuthorityController.LoggedIn += (s, e) =>
            {
                OnPropertyChanged(() => CurrentUser);
            };
        }

        public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnPropertyChanged(() => Visible);
                }
            }
        }

        private bool _metro = true;
        public bool IsMetroTheme
        {
            get
            {
                return _metro;
            }
            set
            {
                if (_metro != value)
                {
                    _metro = value;
                    this.Send(Event.ChangeTheme, value.AsParams(MessageKeys.Boolean));

                    OnPropertyChanged(() => IsMetroTheme);
                }
            }
        }

        public RelayCommand LogoutCommand
        {
            get
            {
                return new RelayCommand(
                    () =>
                    {
                        // viewer.clearhistory()
                        AuthorityController.LogOut();
                    }, () => switcher.Screen != Screen.Login && AuthorityController.CurrentUserCanOpen(Screen.Login));
            }
        }

        public RelayCommand OpenWordsCommand
        {
            get
            {
                return new RelayCommand(
                    () =>
                    {
                        switcher.OpenScreen(Screen.Words);
                    }, () => switcher.Screen != Screen.Words && AuthorityController.CurrentUserCanOpen(Screen.Words));
            }
        }

        public RelayCommand OpenPatientsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    switcher.OpenScreen(Screen.Patients);
                }, () => switcher.Screen != Screen.Patients && AuthorityController.CurrentUserCanOpen(Screen.Patients));
            }
        }

        public RelayCommand AddPatientCommand
        {
            get
            {
                return null;// patProducer.AddPatientCommand;
            }
        }

        public RelayCommand OpenSettingsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.OpenSettings, AuthorityController.CurrentUser.AsParams(MessageKeys.User));
                });
            }
        }
        public RelayCommand OpenHelpCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    // create new window or show it on top
                    if (help == null)
                    {
                        help = new HelpViewModel();
                        help.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == "IsClosed")
                            {
                                help = null;
                            }
                        };

                        this.Send(Event.OpenWindow, help.AsParams(MessageKeys.Window));
                    }
                    else
                    {
                        help.IsActive = true;
                    }
                });
            }
        }

        public IUser CurrentUser
        {
            get { return AuthorityController.CurrentUser; }
        }

        public ToolViewModel SearchPanel { get; private set; }
    }
}
