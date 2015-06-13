using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class LoginTest : ViewModelTest
    {
        LoginViewModel l;

        [TestInitialize]
        public void Init()
        {
            AuthorityController.LogOut();
        }

        [TestCleanup]
        public void Clean()
        {
            if (l != null)
                l.Dispose();
        }

        [TestMethod]
        public void DataConditions()
        {
            Assert.IsFalse(d1.Passport.Remember);
            Assert.IsTrue(d2.Passport.Remember);
        }

        [TestMethod]
        public void AutoLoginRememberedDoctor()
        {
            AuthorityController.AutoLogon = true;
            l = new LoginViewModel();

            Assert.AreEqual(d2, AuthorityController.CurrentDoctor);
        }

        [TestMethod]
        public void NoAutoLoginAfterLogout()
        {
            l = new LoginViewModel();

            Assert.AreEqual(null, AuthorityController.CurrentDoctor);
        }

        [TestMethod]
        public void ChangeRememberedDoctor()
        {
            l = new LoginViewModel();

            l.SelectedUser = d1;
            l.IsRemembered = true;
            l.LoginCommand.Execute(null);

            Assert.IsTrue(d1.Passport.Remember);
            Assert.IsFalse(d2.Passport.Remember);
        }

        [TestMethod]
        public void ShownControlsForAdmin()
        {
            l = new LoginViewModel();

            l.SelectedUser = l.Users.OfType<Admin>().FirstOrDefault();

            Assert.AreEqual(true, l.IsPasswordVisible);
            Assert.AreEqual(false, l.IsRememberVisible);
        }

        [TestMethod]
        public void ShownControlsForDoctor()
        {
            l = new LoginViewModel();

            l.SelectedUser = l.Users.OfType<Doctor>().FirstOrDefault();

            Assert.AreEqual(false, l.IsPasswordVisible);
            Assert.AreEqual(true, l.IsRememberVisible);
        }

        [TestMethod]
        public void SuggestToChangePasswordOnFirstLogin()
        {
            l = new LoginViewModel();

            l.SelectedUser = l.Users.OfType<Admin>().FirstOrDefault();

            Assert.AreEqual(true, l.Passwords.IsRepeatVisible);
        }

        [TestMethod]
        public void DoNotSuggestToChangePasswordWhenItIsNotDefault()
        {
            l = new LoginViewModel();
            var admin = l.Users.OfType<Admin>().FirstOrDefault();
            admin.Passport.HashAndSalt = PasswordHash.PasswordHashManager.CreateHash("111");

            l.SelectedUser = admin;

            Assert.AreEqual(false, l.Passwords.IsRepeatVisible);
        }
    }
}