// using AvalonDock.Layout.Serialization;
using Diagnosis.App.Behaviors;
using Diagnosis.Common;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Screens;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
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
            dockManager.Layout.ElementAdded += (s, e) =>
            {
                logger.DebugFormat("added {0} {1}", e.Element.GetType().Name, "");
            };
            dockManager.Layout.ElementRemoved += (s, e) =>
            {
                logger.DebugFormat("removed {0} {1}", e.Element.GetType().Name, "");
            };
            this.Subscribe(Event.OpenDialog, (e) =>
            {
                var dialogVM = e.GetValue<IDialogViewModel>(MessageKeys.Dialog);
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
            this.Subscribe(Event.OpenWindow, (e) =>
            {
                var windowVm = e.GetValue<IWindowViewModel>(MessageKeys.Window);
                if (windowVm.IsClosed)
                {
                    windowVm.IsActive = true;
                }
                else
                {
                    if (windowVm is HelpViewModel)
                    {
                        ShowWindow(windowVm, new HelpWindow());
                    }
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

        private bool? ShowDialog(IDialogViewModel vm, Window w)
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

        private void ShowWindow(IWindowViewModel vm, Window w)
        {
            w.Owner = this;
            w.DataContext = vm;
            w.Closed += (s, e) =>
            {
                vm.IsClosed = true;
            };
            var path = new PropertyPath(typeof(IWindowViewModel).GetProperty("IsActive"));
            w.SetBinding(ActivateBehavior.IsActiveProperty, new Binding() { Path = path });
            w.Show();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Undo)
                DumpLayout();
        }

        private void DumpLayout()
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
            var SaveLayoutCommand = AvalonDockLayoutSerializer.GetSaveLayoutCommand(dockManager);
            if (SaveLayoutCommand != null)
            {
                string xmlLayoutString = "";

                using (StringWriter fs = new StringWriter())
                {
                    var serializer = new XmlLayoutSerializer(dockManager);
                    serializer.Serialize(fs);

                    xmlLayoutString = fs.ToString();
                }

                if (SaveLayoutCommand is RoutedCommand)
                {
                    (SaveLayoutCommand as RoutedCommand).Execute(xmlLayoutString, dockManager);
                }
                else
                {
                    SaveLayoutCommand.Execute(xmlLayoutString);
                }
            }
        }
    }
}