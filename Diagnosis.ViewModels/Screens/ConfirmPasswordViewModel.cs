using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class ConfirmPasswordViewModel : ScreenBaseViewModel
    {
        private bool _wrongpassword;
        private bool _repPassVis;
        private string _pass;
        private string _passRep;
        private bool passWasSet;
        private bool afterRepeatChanged;

        public string Password
        {
            get { return _pass; }
            set
            {
                _pass = value;
                passWasSet = true;
                afterRepeatChanged = false;
                IsWrongPassword = false;
                OnPropertyChanged(() => Password);
                OnPropertyChanged(() => RepeatMatches);
                OnPropertyChanged(() => PasswordRepeat); // чтобы убрать ошибку для повтора
                OnPropertyChanged(() => CanLogin);
                OnPropertyChanged(() => CanConfirm);
            }
        }

        public string PasswordRepeat
        {
            get { return _passRep; }
            set
            {
                _passRep = value;
                afterRepeatChanged = true;
                OnPropertyChanged(() => PasswordRepeat);
                OnPropertyChanged(() => RepeatMatches);
                OnPropertyChanged(() => CanConfirm);
            }
        }
        /// <summary>
        /// Пароль введен.
        /// </summary>
        public bool CanLogin
        {
            get
            {
                return
                    !Password.IsNullOrEmpty() &&
                    !IsRepeatVisible;
            }
        }

        /// <summary>
        /// Новый пароль сильный и совпадает с повтором.
        /// </summary>
        public bool CanConfirm
        {
            get
            {
                return
                    !Password.IsNullOrEmpty() &&
                    IsRepeatVisible &&
                    PasswordRepeat != null &&
                    AuthorityController.IsStrong(Password) &&
                    RepeatMatches;
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
                    OnPropertyChanged(() => Password);  // чтобы показать ошибку входа
                    OnPropertyChanged(() => IsWrongPassword);
                };
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
                    PasswordRepeat = string.Empty;
                    OnPropertyChanged(() => IsRepeatVisible);
                }
            }
        }

        public bool RepeatMatches
        {
            get { return Password == PasswordRepeat; }
        }

        public override string this[string columnName]
        {
            get
            {
                if (!passWasSet) return string.Empty;

                if (IsWrongPassword && columnName == "Password")
                    return "Неверный пароль";

                if (IsRepeatVisible && columnName == "Password" && !AuthorityController.IsStrong(Password))
                    return "Пароль слабый.";
                if (IsRepeatVisible && columnName == "PasswordRepeat" &&
                    !RepeatMatches && afterRepeatChanged) // не показываем ошибку после смены первого пароля
                    return "Пароли не совпадают.";

                return base[columnName];
            }
        }
    }
}