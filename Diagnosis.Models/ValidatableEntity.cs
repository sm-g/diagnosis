using FluentValidation.Results;
using System;
using System.Collections.Generic;

namespace Diagnosis.Models
{
    [Serializable]
    public abstract class ValidatableEntity<T> : EntityBase<T>, IValidatable
    {
        private ValidationResult resultCache;

        protected ValidatableEntity()
        {
            this.PropertyChanged += (s, e) =>
            {
                resultCache = null;
            };
        }

        public virtual IList<ValidationFailure> GetErrors()
        {
            if (resultCache == null)
            {
                resultCache = SelfValidate();
            }
            return resultCache.Errors;
        }

        public virtual bool IsValid()
        {
            if (resultCache == null)
            {
                resultCache = SelfValidate();
            }
            return resultCache.IsValid;
        }

        public virtual void ResetValidationCache()
        {
            resultCache = null;
        }

        public abstract ValidationResult SelfValidate();
    }

    public interface IValidatable
    {
        ValidationResult SelfValidate();

        IList<ValidationFailure> GetErrors();

        bool IsValid();
    }
}