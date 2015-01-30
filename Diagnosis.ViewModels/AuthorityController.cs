﻿using Diagnosis.Models;
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

        /// <summary>
        /// Меняет пароль пользователю.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns>False, если пароль совпадает с текущим.</returns>
        public static bool ChangePassword(IUser user, SecureString password)
        {
            if (PasswordHashManager.ValidatePassword(password.GetString(), user.Passport.HashAndSalt))
                return false;

            var hash = PasswordHashManager.CreateHash(password.GetString());
            user.Passport.HashAndSalt = hash;
            return true;
        }

        public static bool IsStrong(SecureString password)
        {
            return password.Length > 3;
        }

        [Pure]
        public static bool CurrentUserCanOpen(Screens.Screen screen)
        {
            if (screen == Screens.Screen.Login)
                return true;

            if (CurrentUser is Admin)
            {
                return screen == Screens.Screen.Doctors;
            }
            if (CurrentUser is Doctor)
            {
                return screen != Screens.Screen.Doctors;
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
