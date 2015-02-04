using Diagnosis.Common;
using System;
using System.Linq;

namespace Diagnosis.Models
{
    public class Passport : EntityBase<Guid>, IDomainObject
    {
        private string _hashSalt;
        private bool _rem;
        private Doctor _doctor;

        public Passport(Doctor doc)
        {
            Doctor = doc;
        }

        protected Passport()
        {
        }

        /// <summary>
        /// For 1-1 mapping
        /// </summary>
        public virtual Doctor Doctor
        {
            get { return _doctor; }
            set
            {
                SetProperty(ref _doctor, value, () => Doctor);
            }
        }

        public virtual string HashAndSalt
        {
            get { return _hashSalt; }
            set
            {
                SetProperty(ref _hashSalt, value, () => HashAndSalt);
            }
        }

        public virtual bool Remember
        {
            get { return _rem; }
            set
            {
                SetProperty(ref _rem, value, () => Remember);
            }
        }
    }
}