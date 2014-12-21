using FluentValidation.Results;
using System;

namespace Diagnosis.Models
{
    [Serializable]
    public abstract class ValidatableEntity<T> : EntityBase<T>, IValidatable
    {
        private bool? isValidCache;

        protected ValidatableEntity()
        {
            this.PropertyChanged += (s, e) =>
            {
                isValidCache = null;
            };
        }

        public virtual bool IsValid()
        {
            if (isValidCache == null)
            {
                isValidCache = SelfValidate().IsValid;
            }
            return isValidCache.Value;
        }

        public abstract ValidationResult SelfValidate();
    }

    public interface IValidatable
    {
        ValidationResult SelfValidate();
        bool IsValid();
    }
}