using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Screens
{
    public class MenuBarViewModel : ViewModelBase
    {
        private ScreenSwitcher switcher;
        private PanelViewModel searchAside;

        public MenuBarViewModel(ScreenSwitcher switcher, PanelViewModel searchAside)
        {
            this.switcher = switcher;
            this.searchAside = searchAside;

            AuthorityController.LoggedIn += (s, e) =>
            {
                OnPropertyChanged(() => CurrentUser);
            };
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

        public PanelViewModel SearchAside
        {
            get
            {
                return searchAside;
            }
        }

        public IUser CurrentUser
        {
            get { return AuthorityController.CurrentUser; }
        }
    }
}
