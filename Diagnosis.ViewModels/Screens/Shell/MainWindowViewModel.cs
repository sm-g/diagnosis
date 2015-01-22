using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace Diagnosis.ViewModels.Screens
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(MainWindowViewModel));
        private ScreenSwitcher switcher;
        private SearchViewModel searchPanel;
        private bool? searchVisByUser = null;

        public MainWindowViewModel()
        {
            switcher = new ScreenSwitcher();
            OverlayService = new OverlayServiceViewModel();

            switcher.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CurrentView")
                {
                    MenuBar.Visible = switcher.Screen != Screen.Login;

                    var prevScreen = Panes.FirstOrDefault(p => p.ContentId == ScreenBaseViewModel.ScreenContentId);
                    logger.DebugFormat("CurrentView '{0}' -> '{1}'", prevScreen, CurrentView);

                    // показываем поиск на первом экране, где он может быть
                    // searchPanel.IsVisible = CanShowSearch && (searchVisByUser ?? true);                    

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
                    if (CanShowSearch)
                        // пользователь скрыл/показал поиск, сохраняем
                        searchVisByUser = searchPanel.IsVisible;
                }
            };
            Panes = new ObservableCollection<PaneViewModel>();
            //Panes.Add(searchPanel);
            Panes.CollectionChanged += (s, e) =>
            {
                logger.DebugFormat("Panes {0}", e.Action);
            };

            ADLayout = new AvalonDockLayoutViewModel(ReloadContentOnStartUp);
            MenuBar = new MenuBarViewModel(switcher, searchPanel);
            ADLayout.LayoutLoading += (s, e) =>
            {
                // сначала открываем первый экран
                switcher.OpenScreen(Screen.Login, replace: true);
            };
            ADLayout.LayoutLoaded += (s, e) =>
            {
            };
        }

        private void ReloadContentOnStartUp(LayoutSerializationCallbackEventArgs args)
        {
            string cId = args.Model.ContentId;
            if (string.IsNullOrWhiteSpace(cId) == true)
            {
                args.Cancel = true;
                return;
            }

            var pane = Panes.FirstOrDefault(p => p.ContentId == cId);

            if (pane != null)
                args.Content = pane;
            else
            {
                if (cId == SearchViewModel.ToolContentId)
                {
                    args.Content = searchPanel;
                }
                else if (cId == ScreenBaseViewModel.ScreenContentId)
                {
                    args.Content = CurrentView;
                }

                //args.Content = ReloadDocument(args.Model.ContentId);
                if (args.Content == null)
                    args.Cancel = true;
            }
        }

        public ScreenBaseViewModel CurrentView
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