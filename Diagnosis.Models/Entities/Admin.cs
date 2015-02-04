using Diagnosis.Common;
using System;
using System.Linq;

namespace Diagnosis.Models
{
    /// <summary>
    /// Пользователь-администратор
    /// </summary>
    public class Admin : EntityBase<Guid>, IMan, IUser
    {
        public const string DefaultPassword = "123";
        public static Guid DefaultId = Guid.Parse("3B817ABA-9110-45EF-B81E-A5B975A720DF");
        private const string DefaultLastName = "Администратор";

        public Admin(Passport passport)
            : this()
        {
            Passport = passport;
        }

        protected Admin()
        {
        }

        public override Guid Id
        {
            get { return DefaultId; }
            protected set
            {
            }
        }

        public Passport Passport
        {
            get;
            private set;
        }

        public string LastName
        {
            get { return DefaultLastName; }
            private set { }
        }

        public string FirstName
        {
            get;
            private set;
        }

        public string MiddleName
        {
            get;
            private set;
        }
    }
}