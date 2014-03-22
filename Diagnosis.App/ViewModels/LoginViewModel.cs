using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Security;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private ICommand _loginCommand;
        private string _username;
        private SecureString _password;
        private bool _wrongpassword;
        private DoctorViewModel _selectedDoctor;

        public event EventHandler<LoggedEventArgs> LoggedIn;

        private DoctorsManager _docManager;
        public DoctorsManager DoctorsManager
        {
            get
            {
                return _docManager;
            }
            set
            {
                if (_docManager != value)
                {
                    _docManager = value;
                    OnPropertyChanged(() => DoctorsManager);
                }
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(() => Username);
            }
        }

        public SecureString Password
        {
            private get { return _password; }
            set
            {
                _password = value;
            }
        }

        public bool IsLoginEnabled
        {
            get
            {
                return true; //!String.IsNullOrWhiteSpace(Username ?? "") && Password != null && Password.Length > 0;
            }
        }

        public bool IsWrongPassword
        {
            get { return _wrongpassword; }
            set
            {
                if (_wrongpassword != value)
                {
                    _wrongpassword = value;
                    OnPropertyChanged(() => IsWrongPassword);
                };
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                return _loginCommand ?? (_loginCommand = new RelayCommand(
                    () => LogIn(), () => IsLoginEnabled));
            }
        }

        public LoginViewModel(DoctorsManager manager)
        {
            Contract.Requires(manager != null);
            DoctorsManager = manager;
        }

        private void LogIn()
        {
            // Password.MakeReadOnly();

            // always successful

            var h = LoggedIn;
            if (h != null)
            {
                h(this, new LoggedEventArgs(DoctorsManager.CurrentDoctor));
            }
        }
    }

    public class LoggedEventArgs : EventArgs
    {
        public DoctorViewModel Doctor;

        public LoggedEventArgs(DoctorViewModel doctor)
        {
            Doctor = doctor;
        }
    }
}