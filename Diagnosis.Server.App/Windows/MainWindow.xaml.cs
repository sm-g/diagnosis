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
using Diagnosis.ServerApp.Windows;

namespace Diagnosis.ServerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
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

            DataContext = new ServerMainWindowViewModel();
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
