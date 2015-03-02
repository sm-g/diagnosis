using Diagnosis.ViewModels.Screens;
using System.Windows;

namespace Diagnosis.Client.App.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class AdminSettingsWindow : Window
    {
        public AdminSettingsWindow()
        {
            InitializeComponent();
        }
        private AdminSettingsViewModel Vm { get { return (DataContext as AdminSettingsViewModel); } }

        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender == password)
                Vm.Password = password.SecurePassword;
            else if (sender == passwordRepeat)
                Vm.RepeatPassword = passwordRepeat.SecurePassword;
        }
    }
}