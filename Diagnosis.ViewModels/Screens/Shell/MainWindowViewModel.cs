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
        private ObservableCollection<ScreenBaseViewModel> _screens;
        private ScreenBaseViewModel _curView;
        private IDialogViewModel _modalDialog;
        private string titlePrefix;
        private ScreenSwitcher switcher;

        public MainWindowViewModel(bool demoMode = false)
        {
            if (demoMode)
                titlePrefix = "Демо :: ";

            switcher = new ScreenSwitcher(AuthorityController);

            _screens = new ObservableCollection<ScreenBaseViewModel>();
            Screens = new ReadOnlyObservableCollection<ScreenBaseViewModel>(_screens);
            Tools = new ObservableCollection<ToolViewModel>();
            OverlayService = new OverlayServiceViewModel();
            MenuBar = new MenuBarViewModel(switcher);
            ADLayout = new AvalonDockLayoutViewModel(ReloadContentOnStartUp);

            //DoPanesChangedLogging();

            ADLayout.LayoutLoading += (s, e) =>
            {
                // сначала открываем первый экран, чтобы поставить его в панель во время десериализации лейаута
                switcher.OpenScreen(Screen.Login, replace: true);
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

            if (searchPanel != null)
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

        public string Title
        {
            get { return CurrentView == null ? null : titlePrefix + CurrentView.Title; }
        }

        public MenuBarViewModel MenuBar { get; private set; }

        public OverlayServiceViewModel OverlayService { get; private set; }

        /// <summary>
        /// Экраны как avalon-documents чтобы сворачивать поиск на нужную сторону, а не по ошибке направо.
        /// Fix is here: http://ungorge3.rssing.com/chan-2612661/all_p24.html
        /// </summary>
        public ReadOnlyObservableCollection<ScreenBaseViewModel> Screens { get; private set; }

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
                }, () => searchPanel != null && switcher.WithSearch);
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
            if (anchorable == null)
                return null;

            if (cId == SearchViewModel.ToolContentId)
            {
                // нельзя делать после логина - в авалоне всегда должна быть модель
                searchPanel = new SearchViewModel() { Title = "Поиск" };
                searchPanel.SetAnchorable(anchorable);
                searchPanel.IsVisible = switcher.WithSearch;
                Tools.Add(searchPanel);

                return searchPanel;
            }
            return null;
        }

        private void DoPanesChangedLogging()
        {
            Tools.CollectionChanged += (s, e) =>
            {
                logger.DebugFormat("Tools {0}\n{1}", e.Action, Tools.Aggregate("", (acc, x) => acc + ", " + x));
            };
            _screens.CollectionChanged += (s, e) =>
            {
                logger.DebugFormat("Screens {0}\n{1}", e.Action, _screens.Aggregate("", (acc, x) => acc + ", " + x));
            };
        }

    }
}