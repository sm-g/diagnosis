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
        private readonly Passport _passport;

        public Admin(Passport passport)
            : this()
        {
            _passport = passport;
        }

        protected Admin()
        {
        }

        public override Guid Id
        {
            get { return DefaultId; }
            protected set { }
        }

        public virtual Passport Passport
        {
            get { return _passport; }
        }

        public virtual string LastName
        {
            get { return DefaultLastName; }
            set { }
        }

        public virtual string FirstName
        {
            get;
            set;
        }

        public virtual string MiddleName
        {
            get;
            set;
        }
    }
}