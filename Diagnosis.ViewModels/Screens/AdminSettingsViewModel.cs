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
        private string pass;

        public string Password
        {
            get { return pass; }
            set
            {
                pass = value;
                SetCanOk();
            }
        }

        private string passRep;

        public string PasswordRepeat
        {
            get { return passRep; }
            set
            {
                passRep = value;
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

            new Saver(Session).Save(admin.Passport);
            this.Send(Event.SettingsSaved, admin.AsParams(MessageKeys.User));
        }

        private void SetCanOk()
        {
            CanOk = PasswordRepeat != null && Password != null &&
                AuthorityController.IsStrong(Password) &&
                Password == PasswordRepeat;
        }
    }
}