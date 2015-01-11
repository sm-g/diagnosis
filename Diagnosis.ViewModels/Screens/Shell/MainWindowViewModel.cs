using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(MainWindowViewModel));
        public ScreenSwitcher switcher;
        private SearchViewModel searchPanel;
        private bool? searchVis = null;

        public MainWindowViewModel()
        {
            switcher = new ScreenSwitcher();
            OverlayService = new OverlayServiceViewModel();

            switcher.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CurrentView")
                {
                    MenuBar.Visible = switcher.Screen != Screen.Login;

                    var prevScreen = Panes.FirstOrDefault(p => p.ContentId == "Screen");
                    logger.DebugFormat("CurrentView '{0}' -> '{1}'", prevScreen, CurrentView);

                    // на первом экране поиск видно
                    searchPanel.IsVisible = CanShowSearch && (searchVis.HasValue ? searchVis.Value : true);

                    Panes.Add(CurrentView);
                    Panes.Remove(prevScreen);
                    CurrentView.IsActive = true;

                    OnPropertyChanged("CurrentView");
                }
            };

            searchPanel = new SearchViewModel() { Title = "Поиск", HideAfterInsert = true };
            searchPanel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "IsVisible")
                {
                    if (CanShowSearch) searchVis = searchPanel.IsVisible;
                }
            };
            Panes = new ObservableCollection<PaneViewModel>();
            Panes.Add(searchPanel);
            ADLayout = new AvalonDockLayoutViewModel(Panes);
            MenuBar = new MenuBarViewModel(switcher, searchPanel);

            switcher.OpenScreen(Screen.Login, replace: true);
        }

        public ScreenBase CurrentView
        {
            get { return switcher.CurrentView; }
        }

        public MenuBarViewModel MenuBar { get; private set; }

        public OverlayServiceViewModel OverlayService { get; private set; }

        public ObservableCollection<PaneViewModel> Panes { get; private set; }

        public AvalonDockLayoutViewModel ADLayout { get; private set; }

        public RelayCommand OpenSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    searchPanel.ControlsVisible = true;
                    searchPanel.Activate();
                });
            }
        }

        private bool CanShowSearch
        {
            get
            {
                return (switcher.Screen != Screen.Login) &&
                      (switcher.Screen != Screen.Doctors);
            }
        }
    }
}