using Diagnosis.Common;
using Diagnosis.Data.Specs;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using NHibernate.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class LoginViewModel : ScreenBase
    {
        private IUser _selected;
        private bool _passVis;
        private SecureString _password;
        private bool _remember;
        private bool _rememberVis;
        private bool _wrongpassword;

        public LoginViewModel()
        {
            var doctors = Session.QueryOver<Doctor>().List()
                .OrderBy(d => d.FullName)
                .ToList();

            var adminUser = Session.Query<Passport>().
                Where(u => u.Id == Admin.DefaultId).FirstOrDefault();
            var admin = new Admin(adminUser);

            Users = new ObservableCollection<IUser>(doctors);
            Users.Add(admin);

            Title = "Вход";
            SelectedUser = Users[0];

            if (AuthorityController.AutoLogon)
            {
                var doc = doctors.Where(d => d.Passport.Remember).FirstOrDefault();
                if (doc != null)
                {
                    SelectedUser = doc;
                    DelayedLogon();
                }
            }
        }
        public IUser SelectedUser
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    if (value is Admin)
                    {
                        PasswordVisible = true;
                        RememberVisible = false;
                    }
                    else
                    {
                        PasswordVisible = false;
                        RememberVisible = true;
                    }
                    OnPropertyChanged(() => SelectedUser);
                }
            }
        }

        public ObservableCollection<IUser> Users { get; private set; }

        public bool IsLoginEnabled
        {
            get
            {
                // пароль введен или не нужен
                return !PasswordVisible ||
                    Password != null && Password.Length > 0;
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

        public SecureString Password
        {
            private get { return _password; }
            set
            {
                _password = value;
            }
        }

        public bool PasswordVisible
        {
            get { return _passVis; }
            set
            {
                if (_passVis != value)
                {
                    _passVis = value;
                    OnPropertyChanged(() => PasswordVisible);
                }
            }
        }
        public bool Remember
        {
            get { return _remember; }
            set
            {
                if (_remember != value)
                {
                    _remember = value;
                    OnPropertyChanged(() => Remember);
                }
            }
        }

        public bool RememberVisible
        {
            get { return _rememberVis; }
            set
            {
                if (_rememberVis != value)
                {
                    _rememberVis = value;
                    OnPropertyChanged(() => RememberVisible);
                }
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (Password != null)
                        Password.MakeReadOnly();

                    try
                    {
                        var user = SelectedUser;
                        AuthorityController.LogIn(user, Password, Remember);

                        if (user is Doctor)
                        {
                            // сохраняем remember после входа
                            user.Passport.Remember = Remember;
                            new Diagnosis.Data.Saver(Session).Save(user);
                        }
                    }
                    catch (Exception)
                    {
                        // show error
                    }
                }, () => IsLoginEnabled);
            }
        }

        private void DelayedLogon()
        {
            // need time to complete LoginViewModel ctor and change Current screen
            var timer = new System.Timers.Timer();
            timer.Elapsed += (obj, args) =>
            {
                Remember = true;
                LoginCommand.Execute(null);
            };
            timer.Interval = 100;
            timer.AutoReset = false;
            timer.Start();
        }
    }
}