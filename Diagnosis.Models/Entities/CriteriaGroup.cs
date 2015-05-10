using Diagnosis.Common;
using Diagnosis.Models.Validators;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace Diagnosis.Models
{
    [DebuggerDisplay("CriteriaGroup {Description}")]
    public class CriteriaGroup : ValidatableEntity<Guid>, IDomainObject, ICrit
    {
        private ISet<Criterion> criteria = new HashSet<Criterion>();

        private string _description;

        public CriteriaGroup(Estimator est)
        {
            Estimator = est;
        }

        protected internal CriteriaGroup()
        {
        }

        public virtual event NotifyCollectionChangedEventHandler CriteriaChanged;
        public virtual string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value, () => Description); }
        }
        public virtual Estimator Estimator { get; set; }

        public virtual IEnumerable<Criterion> Criteria
        {
            get { return criteria; }
        }

        public virtual void AddCriterion(Criterion cr)
        {
            criteria.Add(cr);
            OnCriteriaChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, cr));
        }

        public virtual void RemoveCriterion(Criterion cr)
        {
            if (criteria.Remove(cr))
            {
                OnCriteriaChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, cr));
            }
        }

        protected virtual void OnCriteriaChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = CriteriaChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
        public override string ToString()
        {
            return "{0}".FormatStr(Description.Truncate(20));
        }
        public override FluentValidation.Results.ValidationResult SelfValidate()
        {
            return new CriteriaGroupValidator().Validate(this);
        }
    }
}