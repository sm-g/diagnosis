using System;
using System.Collections.ObjectModel;
using EventAggregator;
using Diagnosis.Common;

namespace Diagnosis.ViewModels.Screens
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _menuVisible;
        private bool _searchVis;
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
                    // TODO fix auto open tab when make tabcontrol visible
                    SearchVisible =
                        (switcher.Screen != Screens.Login) &&
                        (switcher.Screen != Screens.Doctors);

                    if ((switcher.Screen & (Screens.Login | Screens.Words)) == switcher.Screen)
                    {
                        RightAside.SearchPanel.Opened = false;
                    }
                    OnPropertyChanged("CurrentView");
                }
            };

            switcher.OpenScreen(Screens.Login, replace: true);
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
        public bool SearchVisible
        {
            get
            {
                return _searchVis;
            }
            set
            {
                if (_searchVis != value)
                {
                    _searchVis = value;
                    OnPropertyChanged(() => SearchVisible);
                }
            }
        }

        public RelayCommand OpenSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RightAside.SearchPanel.Opened = true;
                });
            }
        }
    }
}