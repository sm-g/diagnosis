using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using PasswordHash;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security;
using NHibernate.Linq;
using Diagnosis.Data.Queries;

namespace Diagnosis.ViewModels
{
    public static class AuthorityController
    {
        public static event EventHandler<UserEventArgs> LoggedIn;

        public static event EventHandler LoggedOut;

        private static List<Screen> doctorScreens = new List<Screen> { Screen.Login, Screen.Card, Screen.Patients, Screen.Words };
        private static List<Screen> adminScreens = new List<Screen> { Screen.Login, Screen.Doctors, Screen.Sync, Screen.Vocabularies };

        static AuthorityController()
        {
            AutoLogon = true;
#if DEBUG
            doctorScreens.Add(Screen.Sync);
            doctorScreens.Add(Screen.Vocabularies);
#endif
        }

        public static Doctor CurrentDoctor { get; private set; }

        public static IUser CurrentUser { get; private set; }

        public static bool AutoLogon { get; set; }

        public static bool TryLogIn(IUser user, string password = null)
        {
            if (ValidatePassword(user, password))
            {
                LogOut();
                CurrentUser = user;
                CurrentDoctor = user as Doctor;
                OnLoggedIn(user);

                if (CurrentDoctor != null)
                {
                    var sesion = NHibernateHelper.Default.GetSession();
                    LoadVocsAfterLogin(sesion);
                }
                return true;
            }
            return false;
        }

        public static void LoadVocsAfterLogin(NHibernate.ISession sesion)
        {
            if (CurrentDoctor == null) return;

            var vocsForDoc = VocabularyQuery.NonCustom(sesion)();
            if (CurrentDoctor.Speciality != null)
            {
                vocsForDoc = CurrentDoctor.Speciality.Vocabularies;
            }
            CurrentDoctor.OnLogin(vocsForDoc);
        }

        /// <summary>
        /// Проверяет верный ли пароль для пользователя.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public static bool ValidatePassword(IUser user, string pass)
        {
            if (user is Admin && pass != null)
            {
                // сравниваем хеш пароля в базе с вычисленным по паролю
                return PasswordHashManager.ValidatePassword(pass, user.Passport.HashAndSalt);
            }
            // врач входит без пароля
            return user is Doctor;
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
        public static bool ChangePassword(IUser user, string password)
        {
            if (ValidatePassword(user, password))
                return false;

            var hash = PasswordHashManager.CreateHash(password);
            user.Passport.HashAndSalt = hash;
            return true;
        }

        public static bool IsStrong(string password)
        {
            if (password.IsNullOrEmpty())
                return false;
#if DEBUG
            return password.Length > 1;
#else
            return password.Length > 3 && password != Admin.DefaultPassword;
#endif
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