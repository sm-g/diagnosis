using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using NHibernate.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Windows.Input;
using System.Windows.Threading;

namespace Diagnosis.ViewModels.Screens
{
    public class LoginViewModel : ScreenBaseViewModel
    {
        private IUser _selected;
        private bool _passVis;
        private bool _remember;
        private bool _rememberVis;
        private bool _wrongpassword;
        private bool _repPassVis;

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
                        IsPasswordVisible = true;
                        IsRememberVisible = false;
                        if (AuthorityController.ValidatePassword(value, Admin.DefaultPassword))
                        {
                            // первый вход - надо поменять пароль
                            IsRepeatVisible = true;
                        }
                    }
                    else
                    {
                        IsPasswordVisible = false;
                        IsRememberVisible = true;
                    }
                    OnPropertyChanged(() => SelectedUser);
                }
            }
        }

        public ObservableCollection<IUser> Users { get; private set; }

        public bool CanLogin
        {
            get
            {
                return !IsPasswordVisible || // пароль не нужен
                    !Password.IsNullOrEmpty() && (
                        !IsRepeatVisible || // пароль введен
                        ( // новый пароль
                            PasswordRepeat != null &&
                            AuthorityController.IsStrong(Password) &&
                            Password == PasswordRepeat
                        )
                    );
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


        private string pass;

        public string Password
        {
            get { return pass; }
            set { pass = value; }
        }

        private string passRep;

        public string PasswordRepeat
        {
            get { return passRep; }
            set { passRep = value; }
        }



        public bool IsPasswordVisible
        {
            get { return _passVis; }
            set
            {
                if (_passVis != value)
                {
                    _passVis = value;
                    OnPropertyChanged(() => IsPasswordVisible);
                }
            }
        }

        public bool IsRepeatVisible
        {
            get { return _repPassVis; }
            set
            {
                if (_repPassVis != value)
                {
                    _repPassVis = value;
                    OnPropertyChanged(() => IsRepeatVisible);
                }
            }
        }

        public bool IsRemembered
        {
            get { return _remember; }
            set
            {
                if (_remember != value)
                {
                    _remember = value;
                    OnPropertyChanged(() => IsRemembered);
                }
            }
        }

        public bool IsRememberVisible
        {
            get { return _rememberVis; }
            set
            {
                if (_rememberVis != value)
                {
                    _rememberVis = value;
                    OnPropertyChanged(() => IsRememberVisible);
                }
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var user = SelectedUser;
                    // первый запуск
                    if (IsRepeatVisible && user is Admin)
                    {
                        AuthorityController.ChangePassword(user, Password);
                        new Diagnosis.Data.Saver(Session).Save(user.Passport);
                    }
                    if (AuthorityController.TryLogIn(user, Password))
                    {
                        if (user is Doctor)
                        {
                            // сохраняем remember после входа
                            user.Passport.Remember = IsRemembered;
                            new Diagnosis.Data.Saver(Session).Save(user);
                        }
                    }
                    else
                    {
                        // TODO show login error
                    }
                }, () => CanLogin);
            }
        }

        private void DelayedLogon()
        {
            // need time to complete LoginViewModel ctor and change Current screen
            var timer = new System.Timers.Timer(100);
            var disp = Dispatcher.CurrentDispatcher;
            timer.Elapsed += (obj, args) =>
            {
                disp.Invoke((Action)delegate
                {
                    IsRemembered = true;
                    LoginCommand.Execute(null);
                });
            };
            timer.AutoReset = false;
            timer.Start();
        }
    }
}