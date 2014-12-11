using Diagnosis.Common;
using Diagnosis.Models.Validators;
using FluentValidation.Results;
using Iesi.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class Passport : EntityBase<Guid>, IDomainObject
    {
        string _hashSalt;
        bool _rem;

        public Passport(Doctor doc)
        {

        }

        protected Passport()
        {
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