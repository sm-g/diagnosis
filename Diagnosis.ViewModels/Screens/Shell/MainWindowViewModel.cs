using System;
using System.Collections.ObjectModel;
using EventAggregator;
using Diagnosis.Common;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(MainWindowViewModel));
        public ScreenSwitcher switcher;
        SearchViewModel searchPanel;
        public MainWindowViewModel()
        {
            switcher = new ScreenSwitcher();
            OverlayService = new OverlayServiceViewModel();

            switcher.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CurrentView")
                {
                    MenuBar.Visible = switcher.Screen != Screen.Login;

                    var curScreen = Panes.FirstOrDefault(p => p.ContentId == "Screen");
                    logger.DebugFormat("curScreen {0} -> {1}", curScreen != null ? curScreen.Title : "null", CurrentView.Title);

                    var showSearch =
                        (switcher.Screen != Screen.Login) &&
                        (switcher.Screen != Screen.Doctors);
                    searchPanel.IsVisible = showSearch;

                    Panes.Add(CurrentView);
                    Panes.Remove(curScreen);
                    CurrentView.IsActive = true;


                    OnPropertyChanged("CurrentView");
                }
            };

            searchPanel = new SearchViewModel() { Title = "Поиск", HideOnInsert = true };
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
                    searchPanel.IsVisible = true;
                    searchPanel.IsActive = true;
                });
            }
        }
    }
}