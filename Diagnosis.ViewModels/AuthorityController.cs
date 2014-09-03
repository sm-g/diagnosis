using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels
{
    public static class AuthorityController
    {
        public static event EventHandler LoggedIn;

        public static Doctor CurrentDoctor { get; private set; }

        public static void LogIn(Doctor doctor)
        {
            CurrentDoctor = doctor;

            var h = LoggedIn;
            if (h != null)
            {
                h(typeof(AuthorityController), EventArgs.Empty);
            }
        }
    }
}
