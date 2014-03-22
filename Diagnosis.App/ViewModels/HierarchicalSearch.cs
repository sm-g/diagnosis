using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    public abstract class HierarchicalSearch<T> : SearchBase<T> where T : class, IHierarchical<T>, ICheckable, IEditable
    {
        public bool AllChildren { get; set; }

        public HierarchicalSearch(T parent, bool withNonCheckable = false, bool withChecked = false, bool allChildren = true)
            : base(withNonCheckable, withChecked)
        {
            Contract.Requires(parent != null);

            AllChildren = allChildren;

            Collection = AllChildren ? parent.AllChildren : parent.Children;
        }

        protected override abstract T FromQuery(string query);
        protected override bool CheckConditions(T obj)
        {
            return base.CheckConditions(obj);
        }
        protected override void InitQuery()
        {
            base.InitQuery();
        }
    }
}
