using System;

namespace Diagnosis.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _curScreen;
        private bool _menuVisible;
        private ScreenSwitcher switcher;

        public MainWindowViewModel()
        {
            switcher = new ScreenSwitcher();
            RightAside = new AsideViewModel();
            MenuBar = new MenuBarViewModel(switcher, RightAside.SearchPanel);

            switcher.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CurrentView")
                {
                    MenuVisible = switcher.Screen == Screens.Login;

                    if ((switcher.Screen & (Screens.Login | Screens.Words)) == switcher.Screen)
                    {
                        RightAside.SearchPanel.Opened = false;
                    }
                    OnPropertyChanged("CurrentView");
                }
            };

            AuthorityController.LoggedIn += OnLoggedIn;

            switcher.OpenScreen(Screens.Login, true);
            (CurrentView as LoginViewModel).LoginCommand.Execute(null);
        }

        public ViewModelBase CurrentView
        {
            get
            {
                return switcher.CurrentView;
            }
        }

        public MenuBarViewModel MenuBar { get; private set; }

        public AsideViewModel RightAside { get; private set; }

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