using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Diagnostics;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class MenuBarViewModel : ViewModelBase
    {
        private ScreenSwitcher switcher;
        private bool _visible;
        private VisibleRelayCommand _openSyncCommand;
        private VisibleRelayCommand _openDoctorsCommand;
        private VisibleRelayCommand _openPatientsCommand;
        private VisibleRelayCommand _openWordsCommand;

        public MenuBarViewModel(ScreenSwitcher switcher, SearchViewModel sPanel)
        {
            this.switcher = switcher;

            SearchPanel = sPanel;
            AuthorityController.LoggedIn += (s, e) =>
            {
                OnPropertyChanged(() => CurrentUser);
                OpenSyncCommand.IsVisible = AuthorityController.CurrentUserCanOpen(Screen.Sync);
                OpenDoctorsCommand.IsVisible = AuthorityController.CurrentUserCanOpen(Screen.Doctors);
                OpenPatientsCommand.IsVisible = AuthorityController.CurrentUserCanOpen(Screen.Patients);
                OpenWordsCommand.IsVisible = AuthorityController.CurrentUserCanOpen(Screen.Words);
#if DEBUG
                OpenSyncCommand.IsVisible = true;
#endif
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

        private bool _big;

        public bool IsBigFont
        {
            get
            {
                return _big;
            }
            set
            {
                if (_big != value)
                {
                    _big = value;
                    this.Send(Event.ChangeFont, value.AsParams(MessageKeys.Boolean));

                    OnPropertyChanged(() => IsBigFont);
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

        public RelayCommand OpenDBFolderCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Process.Start(Constants.AppDataDir);
                });
            }
        }

        public VisibleRelayCommand OpenWordsCommand
        {
            get
            {
                return _openWordsCommand ?? (_openWordsCommand = new VisibleRelayCommand(
                    () =>
                    {
                        switcher.OpenScreen(Screen.Words);
                    }, () => switcher.Screen != Screen.Words && AuthorityController.CurrentUserCanOpen(Screen.Words)));
            }
        }

        public VisibleRelayCommand OpenPatientsCommand
        {
            get
            {
                return _openPatientsCommand ?? (_openPatientsCommand = new VisibleRelayCommand(() =>
                {
                    switcher.OpenScreen(Screen.Patients);
                }, () => switcher.Screen != Screen.Patients && AuthorityController.CurrentUserCanOpen(Screen.Patients)));
            }
        }

        public VisibleRelayCommand OpenDoctorsCommand
        {
            get
            {
                return _openDoctorsCommand ?? (_openDoctorsCommand = new VisibleRelayCommand(() =>
                {
                    switcher.OpenScreen(Screen.Doctors);
                }, () => switcher.Screen != Screen.Doctors && AuthorityController.CurrentUserCanOpen(Screen.Doctors)));
            }
        }



        public VisibleRelayCommand OpenSyncCommand
        {
            get
            {
                return _openSyncCommand ?? (_openSyncCommand = new VisibleRelayCommand(() =>
                {
                    switcher.OpenScreen(Screen.Sync);
                }, () => switcher.Screen != Screen.Sync && AuthorityController.CurrentUserCanOpen(Screen.Sync)));
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
                    this.Send(Event.ShowHelp, "".AsParams(MessageKeys.String));
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