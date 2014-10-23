using System;
using System.Collections.ObjectModel;
using EventAggregator;
using Diagnosis.Core;

namespace Diagnosis.ViewModels.Screens
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _menuVisible;
        private ScreenSwitcher switcher;

        public MainWindowViewModel()
        {
            switcher = new ScreenSwitcher();
            RightAside = new AsideViewModel();
            MenuBar = new MenuBarViewModel(switcher, RightAside.SearchPanel);
            OverlayService = new OverlayServiceViewModel();

            switcher.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CurrentView")
                {
                    MenuVisible = switcher.Screen != Screens.Login;

                    if ((switcher.Screen & (Screens.Login | Screens.Words)) == switcher.Screen)
                    {
                        RightAside.SearchPanel.Opened = false;
                    }
                    OnPropertyChanged("CurrentView");
                }
            };

            AuthorityController.LoggedIn += OnLoggedIn;

            switcher.OpenScreen(Screens.Login, replace: true);
            (CurrentView as LoginViewModel).LoginCommand.Execute(null);
        }

        public ScreenBase CurrentView
        {
            get
            {
                return switcher.CurrentView;
            }
        }

        public MenuBarViewModel MenuBar { get; private set; }

        public AsideViewModel RightAside { get; private set; }

        public OverlayServiceViewModel OverlayService { get; private set; }

        public bool MenuVisible
        {
            get
            {
                return _menuVisible;
            }
            set
            {
                if (_menuVisible != value)
                {
                    _menuVisible = value;
                    OnPropertyChanged(() => MenuVisible);
                }
            }
        }
        private void OnLoggedIn(object sender, EventArgs e)
        {
            switcher.OpenScreen(Screens.Patients);
        }
    }
}