using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Search;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
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
        private string _sexes;
        private ScreenBaseViewModel _curView;
        private IDialogViewModel _modalDialog;
        private string titlePrefix;

        public MainWindowViewModel(bool demoMode = false)
        {
            if (demoMode)
            {
                titlePrefix = "Демо :: ";
            }

            switcher = new ScreenSwitcher();
            OverlayService = new OverlayServiceViewModel();

            switcher.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CurrentView")
                {
                    if (CurrentView != null)
                        CurrentView.PropertyChanged -= CurrentView_PropertyChanged;
                    CurrentView = switcher.CurrentView;

                    MenuBar.Visible = switcher.Screen != Screen.Login;

                    var prevScreen = Panes.FirstOrDefault(p => p.ContentId == ScreenBaseViewModel.ScreenContentId);
                    logger.DebugFormat("CurrentView '{0}' -> '{1}'", prevScreen, CurrentView);

                    // показываем поиск на первом экране, где он может быть
                    // searchPanel.IsVisible = CanShowSearch && (searchVisByUser ?? true);

                    Panes.Add(CurrentView);
                    Panes.Remove(prevScreen);

                    CurrentView.IsActive = true;

                    CurrentView.PropertyChanged += CurrentView_PropertyChanged;
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

            AuthorityController.LoggedIn += (s, e) =>
            {
                if (e.user is Doctor)
                {
                    var doc = (Doctor)e.user;
                    Sexes = doc.Settings.SexSigns;

                    this.Send(Event.ChangeFont, doc.Settings.BigFontSize.AsParams(MessageKeys.Boolean));
                }
            };
            this.Subscribe(Event.SettingsSaved, (e) =>
            {
                var doc = e.GetValue<IUser>(MessageKeys.User) as Doctor;
                if (doc != null)
                    Sexes = doc.Settings.SexSigns;
            });

            this.Subscribe(Event.OpenDialog, (e) =>
            {
                var dialogVM = e.GetValue<IDialogViewModel>(MessageKeys.Dialog);
                if (DialogViewModel.ChildWindowModalDialogs.Contains(dialogVM.GetType()))
                {
                    ShowDialog(dialogVM);
                }
            });
        }

        public IDialogViewModel Modal
        {
            get
            {
                return _modalDialog;
            }
            set
            {
                if (_modalDialog != value)
                {
                    _modalDialog = value;
                    OnPropertyChanged(() => Modal);
                }
            }
        }

        public ScreenBaseViewModel CurrentView
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
                    OnPropertyChanged(() => Title);
                }
            }
        }

        public string Title
        {
            get
            {
                return CurrentView == null ? null : titlePrefix + CurrentView.Title;
            }
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

        public RelayCommand FocusOnFilterCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (CurrentView is IFilterableList)
                    {
                        (CurrentView as dynamic).Filter.IsFocused = true;
                    }
                    else
                    {
                        OpenSearchCommand.Execute(null);
                    }
                });
            }
        }

        public string Sexes
        {
            get
            {
                return _sexes;
            }
            set
            {
                if (_sexes != value)
                {
                    _sexes = value;
                    OnPropertyChanged(() => Sexes);
                }
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

        private void ShowDialog(IDialogViewModel vm)
        {
            Contract.Requires(vm != null);
            vm.OnDialogResult(() => Modal = null);
            Modal = vm;
        }

        private void CurrentView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Title")
            {
                OnPropertyChanged(() => Title);
            }
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
                    args.Content = switcher.CurrentView;
                }

                //args.Content = ReloadDocument(args.Model.ContentId);
                if (args.Content == null)
                    args.Cancel = true;
            }
        }
    }
}