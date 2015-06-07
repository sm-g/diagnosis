using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels.Controls;
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

        public LoginViewModel()
        {
            var doctors = EntityQuery<Doctor>.All(Session)()
                .OrderBy(d => d.FullName)
                .ToList();

            var adminPassport = PassportQuery.WithId(Session)(Admin.DefaultId);
            var admin = new Admin(adminPassport);

            Passwords = new ConfirmPasswordViewModel();
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
                    AutoLogIn();
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
                            Passwords.IsRepeatVisible = true;
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

        public ConfirmPasswordViewModel Passwords { get; private set; }


        public bool IsPasswordVisible
        {
            get { return _passVis; }
            set
            {
                if (_passVis != value)
                {
                    _passVis = value;
                    if (!value)
                    {
                        Passwords.Password = string.Empty;
                        Passwords.PasswordRepeat = string.Empty;
                    }
                    OnPropertyChanged(() => IsPasswordVisible);
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
                    if (Passwords.IsRepeatVisible && user is Admin)
                    {
                        AuthorityController.ChangePassword(user, Passwords.Password);
                        Session.DoSave(user.Passport);
                    }
                    if (AuthorityController.TryLogIn(user, Passwords.Password))
                    {
                        if (user is Doctor)
                        {
                            // сохраняем remember после входа
                            user.Passport.Remember = IsRemembered;
                            Session.DoSave(user);
                        }
                        LoggedIn = true;
                    }
                    else
                    {
                        Passwords.IsWrongPassword = true;
                    }
                }, () => !IsPasswordVisible || Passwords.CanLogin || Passwords.CanConfirm);
            }
        }

        public bool LoggedIn { get; set; }

        private void AutoLogIn()
        {
            IsRemembered = true;
            LoginCommand.Execute(null);
        }
    }
}