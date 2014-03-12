using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _loginActive;
        private LoginViewModel _loginContext;
        private ICommand _logout;

        public bool IsLoginActive
        {
            get
            {
                return _loginActive;
            }
            set
            {
                if (_loginActive != value)
                {
                    _loginActive = value;
                    if (value)
                    {
                        MakeLoginDataContext();
                    }

                    OnPropertyChanged(() => IsLoginActive);
                }
            }
        }

        public LoginViewModel Login
        {
            get
            {
                return _loginContext;
            }
            set
            {
                if (_loginContext != value)
                {
                    _loginContext = value;
                    OnPropertyChanged(() => Login);
                }
            }
        }
        public ICommand LogoutCommand
        {
            get
            {
                return _logout ?? (_logout = new RelayCommand(
                                          () =>
                                          {
                                              IsLoginActive = true;
                                          },
                                          () => !IsLoginActive));
            }
        }

        private void MakeLoginDataContext()
        {
            if (Login != null)
            {
                Login.LoggedIn -= OnLoggedIn;
            }
            Login = new LoginViewModel();
            Login.LoggedIn += OnLoggedIn;
        }

        void OnLoggedIn(object sender, LoggedEventArgs e)
        {
            IsLoginActive = false;
        }

        public MainWindowViewModel()
        {
            IsLoginActive = true;
        }
    }
}
