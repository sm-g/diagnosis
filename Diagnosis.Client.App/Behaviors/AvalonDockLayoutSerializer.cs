using Diagnosis.Common;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

// from Edi

namespace Diagnosis.App.Behaviors
{
    /// <summary>
    /// Class implements an attached behavior to load/save a layout for AvalonDock manager.
    /// This layout defines the position and shape of each document and tool window
    /// displayed in the application.
    ///
    /// Load/Save is triggered through command binding
    /// On application start (AvalonDock.Load event results in LoadLayoutCommand) and
    ///    application shutdown (AvalonDock.Unload event results in SaveLayoutCommand).
    ///
    /// This implementation of layout save/load is MVVM compliant, robust, and simple to use.
    /// Just add the following code into your XAML:
    ///
    /// xmlns:AVBehav="clr-namespace:Edi.View.Behavior"
    /// ...
    ///
    /// avalonDock:DockingManager AnchorablesSource="{Binding Tools}"
    ///                           DocumentsSource="{Binding Files}"
    ///                           ActiveContent="{Binding ActiveDocument, Mode=TwoWay, Converter={StaticResource ActiveDocumentConverter}}"
    ///                           Grid.Row="3"
    ///                           SnapsToDevicePixels="True"
    ///                AVBehav:AvalonDockLayoutSerializer.LoadLayoutCommand="{Binding LoadLayoutCommand}"
    ///                AVBehav:AvalonDockLayoutSerializer.SaveLayoutCommand="{Binding SaveLayoutCommand}"
    ///
    /// The LoadLayoutCommand passes a reference of the AvalonDock Manager instance to load the XML layout.
    /// The SaveLayoutCommand passes a string of the XML Layout which can be persisted by the viewmodel/model.
    ///
    /// Both command bindings work with RoutedCommands or delegate commands (RelayCommand).
    /// </summary>
    public static class AvalonDockLayoutSerializer
    {
        private static readonly DependencyProperty LoadLayoutCommandProperty =
            DependencyProperty.RegisterAttached("LoadLayoutCommand",
            typeof(ICommand),
            typeof(AvalonDockLayoutSerializer),
            new PropertyMetadata(null, AvalonDockLayoutSerializer.OnLoadLayoutCommandChanged));

        private static readonly DependencyProperty SaveLayoutCommandProperty =
            DependencyProperty.RegisterAttached("SaveLayoutCommand",
            typeof(ICommand),
            typeof(AvalonDockLayoutSerializer),
            new PropertyMetadata(null, AvalonDockLayoutSerializer.OnSaveLayoutCommandChanged));

        private static readonly DependencyProperty VMProperty =
           DependencyProperty.RegisterAttached("VM",
           typeof(object),
           typeof(AvalonDockLayoutSerializer),
           new PropertyMetadata(null, AvalonDockLayoutSerializer.OnVMChanged));

        #region VM

        public static object GetVM(DependencyObject obj)
        {
            return (object)obj.GetValue(VMProperty);
        }

        public static void SetVM(DependencyObject obj, object value)
        {
            obj.SetValue(VMProperty, value);
        }

        private static void OnVMChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as ViewModels.Screens.AvalonDockLayoutViewModel;
            if (vm != null)
            {
                vm.LayoutFileName = Constants.LayoutFileName;
                vm.DefaultLayout = Diagnosis.App.Properties.Resources.avalon_layout;
                SetLoadLayoutCommand(d, vm.LoadLayoutCommand);
                SetSaveLayoutCommand(d, vm.SaveLayoutCommand);
            }
        }

        #endregion VM

        #region Load Layout

        public static ICommand GetLoadLayoutCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(LoadLayoutCommandProperty);
        }

        public static void SetLoadLayoutCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(LoadLayoutCommandProperty, value);
        }

        private static void OnLoadLayoutCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement framworkElement = d as FrameworkElement;
            framworkElement.Loaded -= OnFrameworkElement_Loaded;

            var command = e.NewValue as ICommand;
            if (command != null)
            {
                framworkElement.Loaded += OnFrameworkElement_Loaded;
            }
        }

        private static void OnFrameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;

            if (frameworkElement == null)
                return;

            ICommand loadLayoutCommand = AvalonDockLayoutSerializer.GetLoadLayoutCommand(frameworkElement);

            // There may not be a command bound to this after all
            if (loadLayoutCommand == null)
                return;

            if (loadLayoutCommand is RoutedCommand)
            {
                (loadLayoutCommand as RoutedCommand).Execute(frameworkElement, frameworkElement);
            }
            else
            {
                loadLayoutCommand.Execute(frameworkElement);
            }
        }

        #endregion Load Layout

        #region Save Layout

        public static ICommand GetSaveLayoutCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(SaveLayoutCommandProperty);
        }

        public static void SetSaveLayoutCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(SaveLayoutCommandProperty, value);
        }

        private static void OnSaveLayoutCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement framworkElement = d as FrameworkElement;
            framworkElement.Unloaded -= OnFrameworkElement_Saved;

            var command = e.NewValue as ICommand;
            if (command != null)
            {
                framworkElement.Unloaded += OnFrameworkElement_Saved; // не сработает, если приложение закрывается
            }

            typeof(AvalonDockLayoutSerializer).Subscribe(Event.Shutdown, (e1) =>
            {
                OnFrameworkElement_Saved(framworkElement, null);
            });
        }

        private static void OnFrameworkElement_Saved(object sender, RoutedEventArgs e)
        {
            DockingManager frameworkElement = sender as DockingManager;
            if (frameworkElement == null)
                return;

            ICommand SaveLayoutCommand = AvalonDockLayoutSerializer.GetSaveLayoutCommand(frameworkElement);

            // There may not be a command bound to this after all
            if (SaveLayoutCommand == null)
                return;

            string xmlLayoutString = "";

            using (StringWriter fs = new StringWriter())
            {
                var serializer = new XmlLayoutSerializer(frameworkElement);
                serializer.Serialize(fs);

                xmlLayoutString = fs.ToString();
            }

            if (SaveLayoutCommand is RoutedCommand)
            {
                (SaveLayoutCommand as RoutedCommand).Execute(xmlLayoutString, frameworkElement);
            }
            else
            {
                SaveLayoutCommand.Execute(xmlLayoutString);
            }
        }

        #endregion Save Layout
    }
}