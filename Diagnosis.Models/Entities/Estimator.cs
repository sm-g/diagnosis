using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Diagnosis.Models
{
    public class Estimator : EntityBase<Guid>, IDomainObject
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

        public virtual void AddCriteriaGroup(CriteriaGroup gr)
        {
            criteriaGroups.Add(gr);
            OnCriteriaGroupChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, gr));
        }

        public virtual void RemoveCriteriaGroup(CriteriaGroup gr)
        {
            if (criteriaGroups.Remove(gr))
            {
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
    }
}