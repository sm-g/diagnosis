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
        private IUser _curUser;
        private bool _visible;
        private VisibleRelayCommand _openSyncCommand;
        private VisibleRelayCommand _openDoctorsCommand;
        private VisibleRelayCommand _openPatientsCommand;
        private VisibleRelayCommand _openWordsCommand;
        private VisibleRelayCommand _openSettingsCommand;
        private VisibleRelayCommand _logoutCommand;
        private VisibleRelayCommand _openVocsCommand;

        public MenuBarViewModel(ScreenSwitcher switcher, SearchViewModel sPanel)
        {
            this.switcher = switcher;

            SearchPanel = sPanel;
            AuthorityController.LoggedIn += (s, e) =>
            {
                CurrentUser = AuthorityController.CurrentUser;

                OpenSyncCommand.IsVisible = AuthorityController.CurrentUserCanOpen(Screen.Sync);
                OpenDoctorsCommand.IsVisible = AuthorityController.CurrentUserCanOpen(Screen.Doctors);
                OpenPatientsCommand.IsVisible = AuthorityController.CurrentUserCanOpen(Screen.Patients);
                OpenWordsCommand.IsVisible = AuthorityController.CurrentUserCanOpen(Screen.Words);
                OpenVocsCommand.IsVisible = AuthorityController.CurrentUserCanOpen(Screen.Vocabularies);
#if DEBUG
                OpenSyncCommand.IsVisible = true;
                OpenVocsCommand.IsVisible = true;
#endif
            };

            AuthorityController.LoggedOut += (s, e) =>
            {
                CurrentUser = AuthorityController.CurrentUser;
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

        public VisibleRelayCommand LogoutCommand
        {
            get
            {
                return _logoutCommand ?? (_logoutCommand = new VisibleRelayCommand(
                    () =>
                    {
                        // viewer.clearhistory()
                        AuthorityController.LogOut();
                    }, () => switcher.Screen != Screen.Login && AuthorityController.CurrentUserCanOpen(Screen.Login)));
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
        public VisibleRelayCommand OpenVocsCommand
        {
            get
            {
                return _openVocsCommand ?? (_openVocsCommand = new VisibleRelayCommand(
                    () =>
                    {
                        switcher.OpenScreen(Screen.Vocabularies);
                    }, () => switcher.Screen != Screen.Vocabularies && AuthorityController.CurrentUserCanOpen(Screen.Vocabularies)));
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

        public VisibleRelayCommand OpenSettingsCommand
        {
            get
            {
                return _openSettingsCommand ?? (_openSettingsCommand = new VisibleRelayCommand(() =>
                {
                    this.Send(Event.OpenSettings, CurrentUser.AsParams(MessageKeys.User));
                }, () => CurrentUser != null));
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
            get
            {
                return _curUser;
            }
            set
            {
                if (_curUser != value)
                {
                    _curUser = value;
                    OpenSettingsCommand.IsVisible = CurrentUser != null;
                    LogoutCommand.IsVisible = CurrentUser != null;
                    OnPropertyChanged(() => CurrentUser);
                }
            }
        }

        public ToolViewModel SearchPanel { get; private set; }
    }
}