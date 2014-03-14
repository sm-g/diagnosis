using System;
using System.Security;
using System.Windows.Input;
using EventAggregator;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Diagnosis.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private ICommand _loginCommand;
        private string _username;
        private SecureString _password;
        private bool _wrongpassword;
        private DoctorViewModel _selectedDoctor;

        public event EventHandler<LoggedEventArgs> LoggedIn;

        public DoctorViewModel SelectedDoctor
        {
            get
            {
                return _selectedDoctor;
            }
            set
            {
                if (_selectedDoctor != value)
                {
                    _selectedDoctor = value;
                    OnPropertyChanged(() => SelectedDoctor);
                }
            }
        }

        public ObservableCollection<DoctorViewModel> Doctors { get; private set; }

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
                return !String.IsNullOrWhiteSpace(Username ?? "") && Password != null && Password.Length > 0;
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

        public LoginViewModel()
        {
            Doctors = new ObservableCollection<DoctorViewModel>(DataCreator.Doctors);
        }

        private void LogIn()
        {
            Password.MakeReadOnly();

            var h = LoggedIn;
            if (h != null)
            {
                h(this, new LoggedEventArgs(SelectedDoctor));
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