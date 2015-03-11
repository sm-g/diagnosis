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
using Diagnosis.ViewModels.Screens;

namespace Diagnosis.ViewModels
{
    public static class AuthorityController
    {
        public static event EventHandler<UserEventArgs> LoggedIn;
        public static event EventHandler LoggedOut;
        static List<Screen> doctorScreens = new List<Screen> { Screen.Login, Screen.Card, Screen.Patients, Screen.Words };
        static List<Screen> adminScreens = new List<Screen> { Screen.Login, Screen.Doctors, Screen.Sync };

        static AuthorityController()
        {
            AutoLogon = true;
        }

        public static Doctor CurrentDoctor { get; private set; }
        public static IUser CurrentUser { get; private set; }
        public static bool AutoLogon { get; set; }
        public static bool LogIn(IUser user, SecureString password = null)
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
        public static bool CurrentUserCanOpen(Screen screen)
        {
            if (CurrentUser is Admin)
            {
                return adminScreens.Contains(screen);
            }
            if (CurrentUser is Doctor)
            {
                return doctorScreens.Contains(screen);
            }
            // nobody can login
            return screen == Screen.Login;
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
