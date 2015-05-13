using Diagnosis.Models.Validators;
using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    [DebuggerDisplay("Estimator {Description}")]
    public class Estimator : ValidatableEntity<Guid>, IDomainObject, ICrit
    {
        private ISet<CriteriaGroup> criteriaGroups = new HashSet<CriteriaGroup>();

        private string _options;
        private string _description;

        public Estimator()
        {

        }

        public virtual event NotifyCollectionChangedEventHandler CriteriaGroupsChanged;

        public virtual string HeaderHrsOptions
        {
            get { return _options; }
            set { SetProperty(ref _options, value, () => HeaderHrsOptions); }
        }

        public virtual string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value, () => Description); }
        }
        public virtual IEnumerable<CriteriaGroup> CriteriaGroups
        {
            get { return criteriaGroups; }
        }

        protected internal virtual void AddCriteriaGroup(CriteriaGroup gr)
        {
            Contract.Requires(gr.Estimator == null);

            if (criteriaGroups.Add(gr))
            {
                gr.Estimator = this;
                OnCriteriaGroupChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, gr));
            }
        }
        public virtual CriteriaGroup AddCriteriaGroup()
        {
            var gr = new CriteriaGroup(this);
            criteriaGroups.Add(gr);
            OnCriteriaGroupChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, gr));
            return gr;
        }
        public virtual void RemoveCriteriaGroup(CriteriaGroup gr)
        {
            if (criteriaGroups.Remove(gr))
            {
                gr.Estimator = null;
                OnCriteriaGroupChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, gr));
            }
        }

        protected virtual void OnCriteriaGroupChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = CriteriaGroupsChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
        public override string ToString()
        {
            return "{0}".FormatStr(Description.Truncate(100));
        }

        public override FluentValidation.Results.ValidationResult SelfValidate()
        {
            return new EstimatorValidator().Validate(this);
        }
    }
}