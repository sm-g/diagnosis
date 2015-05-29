using Diagnosis.Common;
using Diagnosis.Models.Validators;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    [DebuggerDisplay("CriteriaGroup {Description}")]
    public class CriteriaGroup : Crit
    {
        private ISet<Criterion> criteria = new HashSet<Criterion>();

        public CriteriaGroup(Estimator est)
            : base()
        {
            Contract.Requires(est != null);
            Estimator = est;
        }

        protected internal CriteriaGroup()
        {
        }

        public virtual event NotifyCollectionChangedEventHandler CriteriaChanged;

        public virtual Estimator Estimator { get; set; }

        public virtual IEnumerable<Criterion> Criteria
        {
            get { return criteria; }
        }

        protected internal virtual void AddCriterion(Criterion cr)
        {
            Contract.Requires(cr != null);
            Contract.Requires(cr.Group == null);
            if (criteria.Add(cr))
            {
                cr.Group = this;
                OnCriteriaChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, cr));
            }
        }
        public virtual Criterion AddCriterion()
        {
            var cr = new Criterion(this);
            criteria.Add(cr);
            OnCriteriaChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, cr));
            return cr;
        }
        public virtual void RemoveCriterion(Criterion cr)
        {
            if (criteria.Remove(cr))
            {
                cr.Group = null;
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
            return "{0}".FormatStr(Description.Truncate(20)).Replace(Environment.NewLine, " ");
        }
        public override FluentValidation.Results.ValidationResult SelfValidate()
        {
            return new CriteriaGroupValidator().Validate(this);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(Options.IsNullOrEmpty());
        }

    }
}