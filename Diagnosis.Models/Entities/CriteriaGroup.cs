using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using System.Linq;

namespace Diagnosis.Models
{
    public class CriteriaGroup : EntityBase<Guid>, IDomainObject
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
    }
}