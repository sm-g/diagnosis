using System;
using System.Diagnostics;
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
        private DoctorsProducer _docManager;

        public event EventHandler<LoggedEventArgs> LoggedIn;

        public DoctorsProducer DoctorsProducer
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
                    OnPropertyChanged(() => DoctorsProducer);
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

        public LoginViewModel(DoctorsProducer producer)
        {
            Contract.Requires(producer != null);
            DoctorsProducer = producer;
        }

        private void LogIn()
        {
            // Password.MakeReadOnly();

            // always successful

            var h = LoggedIn;
            if (h != null)
            {
                h(this, new LoggedEventArgs(DoctorsProducer.CurrentDoctor));
            }
        }
    }

    public class LoggedEventArgs : EventArgs
    {
        public DoctorViewModel Doctor;

        [DebuggerStepThrough]
        public LoggedEventArgs(DoctorViewModel doctor)
        {
            Doctor = doctor;
        }
    }
}