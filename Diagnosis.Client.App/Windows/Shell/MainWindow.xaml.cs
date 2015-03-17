// using AvalonDock.Layout.Serialization;
using Diagnosis.Client.App.Behaviors;
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
using MahApps.Metro.Controls;
using System.Threading;
using System;

namespace Diagnosis.Client.App.Windows.Shell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(MainWindow));
        private HelpViewModel help;

        public MainWindow(bool demoMode)
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
                else if (dialogVM is IcdSelectorViewModel)
                {
                    ShowDialog(dialogVM, new IcdSelectorWindow());
                }
                else if (dialogVM is MeasureEditorViewModel)
                {
                    ShowDialog(dialogVM, new EditorWindow());
                }
            });

            this.Subscribe(Event.ShowHelp, (e) =>
            {
                var topic = e.GetValue<string>(MessageKeys.String);
                // create new window or show it on top
                if (help == null)
                {
                    help = new HelpViewModel(topic);
                    help.PropertyChanged += (s, e1) =>
                    {
                        if (e1.PropertyName == "IsClosed")
                        {
                            help = null;
                        }
                    };
                    // to access help form modal dialog
                    // https://eprystupa.wordpress.com/2008/07/28/running-wpf-application-with-multiple-ui-threads/
                    var thread = new Thread(() =>
                    {
                        var window = new HelpWindow();
                        ShowWindow(help, window, false);

                        this.Closed += (s, e1) =>
                        {
                            window.Dispatcher.Invoke((Action)(() =>
                            {
                                window.Close();
                            }));
                        };
                        window.Closed += (s, e2) => window.Dispatcher.InvokeShutdown();
                        System.Windows.Threading.Dispatcher.Run();
                    });

                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
                else
                {
                    help.Topic = topic;
                    help.IsActive = true;
                }

            });

            Loaded += (s, e) =>
            {
#if DEBUG
                // debugMenu.Visibility = System.Windows.Visibility.Visible;
#endif

                if (demoMode)
                {
#if !DEBUG
                new Thread(new ThreadStart(delegate
                {
                    MessageBox.Show(
                        "Проверьте строку подключения в файле '{0}'".FormatStr(Constants.ClientConfigFilePath),
                        "Демонстрационный режим",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                })).Start();
#endif
                }
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

        MainWindowViewModel Vm { get { return DataContext as MainWindowViewModel; } }

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

        private void ShowWindow(IWindowViewModel vm, Window w, bool setOwner)
        {
            if (setOwner)
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly()
                                          .GetName()
                                          .Version
                                          .ToString();
            MessageBox.Show(version, "Версия", MessageBoxButton.OK);
        }
    }
}