using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using Diagnosis.Common;
using PasswordHash;
using System.Diagnostics.Contracts;

namespace Diagnosis.ViewModels
{
    public static class AuthorityController
    {
        public static event EventHandler<UserEventArgs> LoggedIn;
        public static event EventHandler LoggedOut;
        static AuthorityController()
        {
            AutoLogon = true;
        }

        public static Doctor CurrentDoctor { get; private set; }
        public static IUser CurrentUser { get; private set; }
        public static bool AutoLogon { get; set; }
        public static bool LogIn(IUser user, SecureString password = null, bool remember = false)
        {
            if (user is Admin)
            {
                // сравниваем хеш пароля в базе с вычисленным по паролю
                if (PasswordHashManager.ValidatePassword(password.GetString(), user.Passport.HashAndSalt))
                {
                    CurrentUser = user;
                    OnLoggedIn(user);
                    return true;
                }
                return false;

            }
            else // (user is Doctor) // врач входит без пароля
            {
                CurrentDoctor = user as Doctor;
                CurrentUser = user;
                OnLoggedIn(user);
                return true;
            }

        }

        public static void LogOut()
        {
            AutoLogon = false;
            CurrentDoctor = null;
            CurrentUser = null;
            OnLoggedOut();
        }

        [Pure]
        public static bool CurrentUserCanOpen(Screens.Screens screen)
        {
            if (screen == Screens.Screens.Login)
                return true;

            if (CurrentUser is Admin)
            {
                return screen == Screens.Screens.Doctors;
            }
            if (CurrentUser is Doctor)
            {
                return screen != Screens.Screens.Doctors;
            }
            return false;
        }

        private static void OnLoggedIn(IUser user)
        {
            var h = LoggedIn;
            if (h != null)
            {
                h(typeof(AuthorityController), new UserEventArgs(user));
            }
        }
        private static void OnLoggedOut()
        {
            var h = LoggedOut;
            if (h != null)
            {
                h(typeof(AuthorityController), EventArgs.Empty);
            }
        }
    }
}
