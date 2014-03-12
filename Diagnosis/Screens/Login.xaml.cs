﻿using Diagnosis.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.Screens
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
        }

        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            (DataContext as LoginViewModel).Password = password.SecurePassword;
        }
    }
}