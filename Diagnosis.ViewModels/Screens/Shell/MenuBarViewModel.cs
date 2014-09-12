using Diagnosis.Core;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels
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
                OnPropertyChanged(() => CurrentDoctor);
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
                        switcher.OpenScreen(Screens.Login);
                    }, () => switcher.Screen != Screens.Login);
            }
        }

        public RelayCommand OpenWordsCommand
        {
            get
            {
                return new RelayCommand(
                    () =>
                    {
                        switcher.OpenScreen(Screens.Words);
                    }, () => switcher.Screen != Screens.Words);
            }
        }

        public RelayCommand OpenPatientsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    switcher.OpenScreen(Screens.Patients);
                }, () => switcher.Screen != Screens.Patients);
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
                    var settingsVM = new SettingsViewModel(AuthorityController.CurrentDoctor);
                    this.Send(Events.OpenSettings, settingsVM.AsParams(MessageKeys.Settings));
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

        public Doctor CurrentDoctor
        {
            get { return AuthorityController.CurrentDoctor; }
        }
    }
}
