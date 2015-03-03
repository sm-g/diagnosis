using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Diagnosis.ViewModels.Screens;
using EventAggregator;
using Diagnosis.Common;
using Diagnosis.ViewModels;
using Diagnosis.Server.App.Windows;
using System.Threading;

namespace Diagnosis.Server.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(bool demoMode = false)
        {
            InitializeComponent();
            this.Subscribe(Event.OpenDialog, (e) =>
            {
                var dialogVM = e.GetValue<IDialogViewModel>(MessageKeys.Dialog);
                if (dialogVM is UomEditorViewModel)
                {
                    ShowDialog(dialogVM, new UomEditorWindow());
                }
            });

            Loaded += (s, e) =>
            {
                if (demoMode)
                {
#if !DEBUG
                    new Thread(new ThreadStart(delegate
                    {
                        MessageBox.Show(
                            "Проверьте строку подключения в файле '{0}'".FormatStr(Constants.ServerConfigFilePath),
                            "Демонстрационный режим",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    })).Start();
#endif
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

    }
}
