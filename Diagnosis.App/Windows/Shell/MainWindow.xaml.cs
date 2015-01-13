using Diagnosis.Common;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.App.Controls;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Diagnosis.App.Windows.Shell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(MainWindow));
        public MainWindow()
        {

            InitializeComponent();

            this.Subscribe(Event.OpenDialog, (e) =>
            {
                var dialogVM = e.GetValue<IDialog>(MessageKeys.Dialog);
                if (dialogVM is PatientEditorViewModel)
                {
                    ShowDialog(dialogVM, new PatientEditorWindow());
                }
                else if (dialogVM is CourseEditorViewModel)
                {
                    ShowDialog(dialogVM, new CourseEditorWindow());
                }
                else if (dialogVM is AppointmentEditorViewModel)
                {
                    ShowDialog(dialogVM, new AppointmentEditorWindow());
                }
                else if (dialogVM is SettingsViewModel)
                {
                    ShowDialog(dialogVM, new SettingsWindow());
                }
                else if (dialogVM is WordEditorViewModel)
                {
                    ShowDialog(dialogVM, new WordEditorWindow());
                }
                else if (dialogVM is DoctorEditorViewModel)
                {
                    ShowDialog(dialogVM, new DoctorEditorWindow());
                }
                else if (dialogVM is IcdSelectorViewModel)
                {
                    ShowDialog(dialogVM, new IcdSelectorWindow());
                }
                else if (dialogVM is MeasureEditorViewModel)
                {
                    ShowDialog(dialogVM, new MeasureEditorWindow());
                }
            });

            DataContext = new MainWindowViewModel();
            Loaded += (s, e) =>
            {
#if DEBUG
                // debugMenu.Visibility = System.Windows.Visibility.Visible;
#endif

            };
            dockManager.ActiveContentChanged += (s, e) =>
            {
                var pane = dockManager.ActiveContent as PaneViewModel;
                if (pane != null)
                {
                    logger.DebugFormat("AD active = {0} ", pane);
                }
            };
        }

        private bool? ShowDialog(IDialog vm, Window w)
        {
            w.Owner = this;
            w.Closing += (s, e) =>
            {
                if (vm.DialogResult == null)
                {
                    vm.CancelCommand.Execute(null);
                }
            };
            w.DataContext = vm;
            var result = w.ShowDialog();
            return result;
        }
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Undo)
                DumpLayout();
        }
        void DumpLayout()
        {
            //List<DependencyObject> scopes = new List<DependencyObject>();
            //foreach (var item in this.FindVisualChildren())
            //{
            //    scopes.Add(FocusManager.GetFocusScope(item));
            //}
            //scopes = scopes.Distinct().ToList();
            //foreach (var item in scopes)
            //{
            //    logger.DebugFormat("scope - {0}", item);

            //}
            //var grid = this.FindChild<Grid>("grid");
            //var rec = this.FindChild<ListBox>("records");
            //if (grid != null)
            //    logger.DebugFormat("grid Focused {0}", grid.IsFocused);
            //logger.DebugFormat("rec Focused {0}", rec.IsFocused);

            dockManager.Layout.ConsoleDump(0);
        }
    }
}