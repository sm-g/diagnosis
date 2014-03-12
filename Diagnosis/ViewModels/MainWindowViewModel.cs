using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _loginActive;
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
                        Login = new LoginViewModel();
                        Login.LoggedIn += LoginDataContext_LoggedIn;
                    }

                    OnPropertyChanged(() => IsLoginActive);
                }
            }
        }

        void LoginDataContext_LoggedIn(object sender, LoggedEventArgs e)
        {
            IsLoginActive = false;
        }


        private LoginViewModel _loginContext;
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

        public MainWindowViewModel()
        {
            IsLoginActive = true;
        }
    }
}
