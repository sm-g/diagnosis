using Diagnosis.ViewModels.Screens;
using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.Client.App.Screens
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                Vm.PropertyChanged += Vm_PropertyChanged;
            };
            Unloaded += (s, e) =>
            {
                Vm.PropertyChanged -= Vm_PropertyChanged;
            };
        }

        private LoginViewModel Vm { get { return (DataContext as LoginViewModel); } }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PasswordVisibility" && !Vm.PasswordVisible)
            {
                password.Clear();
            }
        }
        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Vm.Password = password.SecurePassword;
        }
    }
}