﻿using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace Diagnosis.ViewModels.Screens
{
    public class MenuBarViewModel : ViewModelBase
    {
        private ScreenSwitcher switcher;
        private bool _visible;

        public MenuBarViewModel(ScreenSwitcher switcher, SearchViewModel sPanel)
        {
            this.switcher = switcher;

            SearchPanel = sPanel;
            AuthorityController.LoggedIn += (s, e) =>
            {
                OnPropertyChanged(() => CurrentUser);
                OpenSyncCommand.IsVisible = CurrentUser is Admin;
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
        VisibleRelayCommand _openSyncCommand;
        public VisibleRelayCommand OpenSyncCommand
        {
            get
            {
                return _openSyncCommand ?? (_openSyncCommand = new VisibleRelayCommand(() =>
                {
                    switcher.OpenScreen(Screen.Sync);

                },
                () => switcher.Screen != Screen.Sync && AuthorityController.CurrentUserCanOpen(Screen.Sync)));
            }
        }

        public RelayCommand AddCommand
        {
            get
            {
                return null;// patProducer.AddCommand;
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
