using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels.Controls;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security;

namespace Diagnosis.ViewModels.Screens
{
    public class AdminSettingsViewModel : DialogViewModel
    {
        private readonly Admin admin;

        public AdminSettingsViewModel(Admin admin)
        {
            Contract.Requires(admin != null);
            this.admin = admin;
            Passwords = new ConfirmPasswordViewModel()
            {
                IsRepeatVisible = true
            };

            CanOk = false;
            Passwords.PropertyChanged += (s, e) =>
            {
                CanOk = Passwords.CanConfirm;
            };

            Title = "Настройки";
            HelpTopic = "adminsettings";
            WithHelpButton = false;
        }

        protected override void OnOk()
        {
            AuthorityController.ChangePassword(admin, Passwords.Password);

            Session.DoSave(admin.Passport);
            this.Send(Event.EntitySaved, admin.AsParams(MessageKeys.Entity));
        }

        public ConfirmPasswordViewModel Passwords { get; private set; }
    }
}