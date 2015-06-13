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
    [DebuggerDisplay("Estimator {Description}")]
    public class Estimator : CritBase
    {
        private ISet<CriteriaGroup> criteriaGroups = new HashSet<CriteriaGroup>();

        public Estimator()
            : base()
        {
        }

        public virtual event NotifyCollectionChangedEventHandler CriteriaGroupsChanged;

        public virtual IEnumerable<CriteriaGroup> CriteriaGroups
        {
            get { return criteriaGroups; }
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

        protected internal virtual void AddCriteriaGroup(CriteriaGroup gr)
        {
            Contract.Requires(gr != null);
            Contract.Requires(gr.Estimator == null);

            if (criteriaGroups.Add(gr))
            {
                gr.Estimator = this;
                OnCriteriaGroupChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, gr));
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
            return "{0}".FormatStr(Description.Truncate(20)).Replace(Environment.NewLine, " ");
        }

        public override FluentValidation.Results.ValidationResult SelfValidate()
        {
            return new EstimatorValidator().Validate(this);
        }
    }
}