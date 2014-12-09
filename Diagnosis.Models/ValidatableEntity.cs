using FluentValidation.Results;

namespace Diagnosis.Models
{
    public abstract class ValidatableEntity<T> : EntityBase<T>
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
}