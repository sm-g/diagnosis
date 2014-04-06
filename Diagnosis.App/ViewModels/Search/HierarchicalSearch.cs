using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    public abstract class HierarchicalSearch<T> : SearchBase<T> where T : class, IHierarchical<T>, ICheckable
    {
        protected T Parent { get; private set; }
        public bool AllChildren { get; set; }

        public HierarchicalSearch(T parent, bool withNonCheckable = false, bool withChecked = false, bool withCreatingNew = true, bool allChildren = true)
            : base(withNonCheckable, withChecked, withCreatingNew)
        {
            Contract.Requires(parent != null);

            AllChildren = allChildren;
            Parent = parent;

            Collection = AllChildren ? parent.AllChildren : parent.Children;
        }
    }
}
