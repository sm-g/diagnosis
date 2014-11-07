using Diagnosis.Common;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using System.Windows;

namespace Diagnosis.App.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Subscribe(Events.OpenSettings, (e) =>
            {
                var settingsVM = e.GetValue<IDialog>(MessageKeys.Dialog);
                var settingsDialog = new SettingsWindow();
                ShowDialog(settingsVM, settingsDialog);
            });
            this.Subscribe(Events.OpenHolderEditor, (e) =>
            {
                var dialogVM = e.GetValue<IDialog>(MessageKeys.Dialog);
                if (dialogVM is CourseEditorViewModel)
                {
                    ShowDialog(dialogVM, new CourseEditorWindow());
                }
                else if (dialogVM is AppointmentEditorViewModel)
                {
                    ShowDialog(dialogVM, new AppointmentEditorWindow());
                }
            });

            DataContext = new MainWindowViewModel();
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
    }
}