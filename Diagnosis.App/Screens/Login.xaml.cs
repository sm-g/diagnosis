using Diagnosis.ViewModels.Screens;
using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.App.Screens
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
                Vm.PropertyChanged += (s1, e1) =>
                {
                    if (e1.PropertyName == "PasswordVisibility" && !Vm.PasswordVisible)
                    {
                        password.Clear();
                    }
                };
            };
           
        }

        LoginViewModel Vm { get { return (DataContext as LoginViewModel); } }

        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Vm.Password = password.SecurePassword;
        }
    }
}