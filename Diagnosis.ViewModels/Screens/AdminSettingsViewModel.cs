using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security;

namespace Diagnosis.ViewModels.Screens
{
    public class AdminSettingsViewModel : DialogViewModel
    {
        private readonly Admin admin;

        private SecureString _password;
        private bool _repPassVis;
        private SecureString _repeatPassword;

        public AdminSettingsViewModel(Admin admin)
        {
            Contract.Requires(admin != null);
            this.admin = admin;
            IsRepeatVisible = true;
            CanOk = false;
        }

        public SecureString Password
        {
            get { return _password; }
            set
            {
                _password = value;
                SetCanOk();
            }
        }

        public SecureString RepeatPassword
        {
            get { return _repeatPassword; }
            set
            {
                _repeatPassword = value;
                SetCanOk();
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

        protected override void OnOk()
        {
            AuthorityController.ChangePassword(admin, Password);

            new Saver(Session).Save(admin);
            this.Send(Event.SettingsSaved, admin.AsParams(MessageKeys.User));
        }

        private void SetCanOk()
        {
            CanOk = RepeatPassword != null && Password != null &&
                AuthorityController.IsStrong(Password) &&
                Password.GetString() == RepeatPassword.GetString();
        }
    }
}