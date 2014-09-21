using FluentValidation.Results;

namespace Diagnosis.Models
{
    public abstract class ValidatableEntity : EntityBase
    {
        private bool? isValidCache;

        protected ValidatableEntity()
        {
            this.PropertyChanged += (s, e) =>
            {
                isValidCache = null;
            };
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
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