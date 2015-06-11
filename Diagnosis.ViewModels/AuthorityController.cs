using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using PasswordHash;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public sealed class AuthorityController
    {
        private static readonly System.Lazy<AuthorityController> lazyInstance = new System.Lazy<AuthorityController>(() => new AuthorityController());

        public static event EventHandler<UserEventArgs> LoggedIn;

        public static event EventHandler LoggedOut;

        private List<Screen> doctorScreens = new List<Screen> { Screen.Login, Screen.Card, Screen.Patients, Screen.Words, Screen.Criteria };
        private List<Screen> adminScreens = new List<Screen> { Screen.Login, Screen.Doctors, Screen.Sync, Screen.Vocabularies };

        private AuthorityController()
        {
            AutoLogon = true;
#if DEBUG
            doctorScreens.Add(Screen.Sync);
            doctorScreens.Add(Screen.Vocabularies);
#endif
        }

        public static AuthorityController Default { get { return lazyInstance.Value; } }

        public Doctor CurrentDoctor { get; private set; }

        public IUser CurrentUser { get; private set; }

        public bool AutoLogon { get; set; }

        public bool TryLogIn(IUser user, string password = null)
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

        public void LoadVocsAfterLogin(NHibernate.ISession session)
        {
            if (CurrentDoctor == null) return;

            var vocsForDoc = VocabularyQuery.NonCustom(session)();
            if (CurrentDoctor.Speciality != null)
            {
                vocsForDoc = CurrentDoctor.Speciality.Vocabularies;
            }
            CurrentDoctor.CacheSpecialityVocs(vocsForDoc);
        }

        /// <summary>
        /// Проверяет верный ли пароль для пользователя.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public bool ValidatePassword(IUser user, string pass)
        {
            if (user is Admin && pass != null)
            {
                // сравниваем хеш пароля в базе с вычисленным по паролю
                return PasswordHashManager.ValidatePassword(pass, user.Passport.HashAndSalt);
            }
            // врач входит без пароля
            return user is Doctor;
        }

        public void LogOut()
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
        public bool ChangePassword(IUser user, string password)
        {
            if (ValidatePassword(user, password))
                return false;

            var hash = PasswordHashManager.CreateHash(password);
            user.Passport.HashAndSalt = hash;
            return true;
        }

        public bool IsStrong(string password)
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
        public bool CurrentUserCanOpen(Screen screen)
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

        private void OnLoggedIn(IUser user)
        {
            var h = LoggedIn;
            if (h != null)
            {
                h(this, new UserEventArgs(user));
            }
        }

        private void OnLoggedOut()
        {
            var h = LoggedOut;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }
    }
}