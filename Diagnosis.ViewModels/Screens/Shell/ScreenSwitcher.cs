using System;
using System.Windows.Navigation;

namespace Diagnosis.ViewModels
{
    [Flags]
    public enum Screens
    {
        Login, Patients, Words, Card
    }

    public class ScreenSwitcher : ViewModelBase
    {
        private Screens _curScreen;
        private ViewModelBase _curView;
        private LoginViewModel _login;
        private PatientsListViewModel _patients;
        private WordsListViewModel _words;

        public ScreenSwitcher()
        {
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
                    _curScreen = value;
                    OnPropertyChanged(() => Screen);
                }
            }
        }

        public ViewModelBase CurrentView
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

        public LoginViewModel Login
        {
            get
            {
                return _login ?? (_login = new LoginViewModel());
            }
        }

        public PatientsListViewModel Patients
        {
            get { return _patients ?? (_patients = new PatientsListViewModel()); }
        }

        public WordsListViewModel Words
        {
            get { return _words ?? (_words = new WordsListViewModel()); }
        }

        public void OpenScreen(Screens screen)
        {
            Screen = screen;
            switch (screen)
            {
                case Diagnosis.ViewModels.Screens.Login:
                    CurrentView = new LoginViewModel();
                    break;

                case Diagnosis.ViewModels.Screens.Patients:
                    CurrentView = new PatientsListViewModel();
                    break;

                case Diagnosis.ViewModels.Screens.Words:
                    CurrentView = new WordsListViewModel();
                    break;

                case Diagnosis.ViewModels.Screens.Card:
                    CurrentView = new CardViewModel();
                    break;

                default:
                    break;
            }

            if (screen != Screens.Card)
            {
                CardViewModel.viewer.ClosePatient();
            }
        }

    }
}
