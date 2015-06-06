using Diagnosis.Common;
using Diagnosis.Models;
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
        private SearchViewModel searchPanel;
        private string _sexes;
        private ScreenBaseViewModel _curView;
        private IDialogViewModel _modalDialog;
        private string titlePrefix;
        private ScreenSwitcher switcher;
        private bool loaded;

        public MainWindowViewModel(bool demoMode = false)
        {
            if (demoMode)
                titlePrefix = "Демо :: ";

            switcher = new ScreenSwitcher(AuthorityController);

            OverlayService = new OverlayServiceViewModel();
            MenuBar = new MenuBarViewModel(switcher);
            Tools = new ObservableCollection<ToolViewModel>();
            ADLayout = new AvalonDockLayoutViewModel(ReloadContentOnStartUp);

            // нельзя делать после логина - в авалоне всегда должна быть модель
            searchPanel = new SearchViewModel() { Title = "Поиск" };

            Tools.Add(searchPanel);

            ADLayout.LayoutLoading += (s, e) =>
            {
                // сначала открываем первый экран, чтобы поставить его в панель во время десериализации лейаута
                switcher.OpenScreen(Screen.Login, replace: true);
            };
            ADLayout.LayoutLoaded += (s, e) =>
            {
                loaded = true;
            };

            Subscribe();
        }

        private void OnCurrentViewChanged()
        {
            // показываем новый экран

            if (CurrentView != null)
                CurrentView.PropertyChanged -= CurrentView_PropertyChanged;

            CurrentView = switcher.CurrentView;
            CurrentView.PropertyChanged += CurrentView_PropertyChanged;

            if (loaded) // уже есть панель
                searchPanel.IsVisible = switcher.WithSearch;

            var prevScreen = _screens.FirstOrDefault(p => p.ContentId == ScreenBaseViewModel.ScreenContentId);
            _screens.Add(CurrentView);
            _screens.Remove(prevScreen);
            CurrentView.IsActive = true;
        }

        private void Subscribe()
        {
            switcher.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CurrentView")
                {
                    OnCurrentViewChanged();
                }
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
            get { return _modalDialog; }
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
            get { return _curView; }
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

        ObservableCollection<ScreenBaseViewModel> _screens = new ObservableCollection<ScreenBaseViewModel>();
        ReadOnlyObservableCollection<ScreenBaseViewModel> _readonlyScreens = null;
        public ReadOnlyObservableCollection<ScreenBaseViewModel> Screens
        {
            get
            {
                if (_readonlyScreens == null)
                    _readonlyScreens = new ReadOnlyObservableCollection<ScreenBaseViewModel>(_screens);

                return _readonlyScreens;
            }
        }
        public string Title
        {
            get { return CurrentView == null ? null : titlePrefix + CurrentView.Title; }
        }

        public MenuBarViewModel MenuBar { get; private set; }

        public OverlayServiceViewModel OverlayService { get; private set; }

        public ObservableCollection<ToolViewModel> Tools { get; private set; }

        public AvalonDockLayoutViewModel ADLayout { get; private set; }

        public RelayCommand OpenSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    searchPanel.ControlsVisible = true;
                    searchPanel.Activate();
                }, () => switcher.WithSearch);
            }
        }

        public RelayCommand FocusOnFilterCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (CurrentView is IFilterableList)
                        (CurrentView as IFilterableList).Filter.IsFocused = true;
                    else if (OpenSearchCommand.CanExecute(null))
                        OpenSearchCommand.Execute(null);
                });
            }
        }

        public string Sexes
        {
            get { return _sexes; }
            set
            {
                if (_sexes != value)
                {
                    _sexes = value;
                    OnPropertyChanged(() => Sexes);
                }
            }
        }

        private void ShowDialog(IDialogViewModel vm)
        {
            Contract.Requires(vm != null);
            vm.OnDialogResult((result) => Modal = null);
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
            logger.DebugFormat("ReloadContentOnStartUp, {0}", cId);

            var pane = Tools.Union<PaneViewModel>(Screens).FirstOrDefault(p => p.ContentId == cId);
            if (pane != null)
                args.Content = pane;
            else
                args.Content =
                    ReloadTool(args.Model) as PaneViewModel ??
                    ReloadDocument(args.Model) as PaneViewModel;

            // убираем пустую панель
            if (args.Content == null)
                args.Cancel = true;
        }

        private ScreenBaseViewModel ReloadDocument(Xceed.Wpf.AvalonDock.Layout.LayoutContent model)
        {
            string cId = model.ContentId;
            if (cId == ScreenBaseViewModel.ScreenContentId)
            {
                return CurrentView;
            }

            return null;
        }

        private ToolViewModel ReloadTool(Xceed.Wpf.AvalonDock.Layout.LayoutContent model)
        {
            string cId = model.ContentId;
            var anchorable = model as Xceed.Wpf.AvalonDock.Layout.LayoutAnchorable;

            if (cId == SearchViewModel.ToolContentId)
            {
                searchPanel.SetAnchorable(anchorable);

                searchPanel.IsVisible = switcher.WithSearch; // теперь можно скрыть панель

                searchPanel.SetIsAutoHiddenChangingCallback((willBeAutoHidden) =>
                {
                    logger.DebugFormat("AutoHiddenChangingCallback from Reload. IsAutoHidden {0}, IsHidden {1}, willBeAutoHidden {2}",
                        anchorable.IsAutoHidden, anchorable.IsHidden, willBeAutoHidden);
                    if (willBeAutoHidden != anchorable.IsAutoHidden)
                    {
                        anchorable.ToggleAutoHide();
                    }
                });
                return searchPanel;
            }
            return null;
        }
    }
}