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

            this.Subscribe(Events.OpenDialog, (e) =>
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
                else if (dialogVM is IcdSelectorViewModel)
                {
                    ShowDialog(dialogVM, new IcdSelectorWindow());
                }
                else if (dialogVM is WordEditorViewModel)
                {
                    ShowDialog(dialogVM, new WordEditorWindow());
                }
                else if (dialogVM is DoctorEditorViewModel)
                {
                    ShowDialog(dialogVM, new DoctorEditorWindow());
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