using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Screens
{
    public class HrCategoryViewModel : CheckableBase, IComparable
    {
        internal readonly HrCategory category;

        public HrCategoryViewModel(HrCategory category)
        {
            Contract.Requires(category != null);
            this.category = category;
        }

        public string Name
        {
            get
            {
                return category.Title;
            }
        }
        public int CompareTo(object obj)
        {
            var other = obj as HrCategoryViewModel;
            if (other == null)
                return -1;

            return this.category.CompareTo(other.category);
        }

        public override string ToString()
        {
            return category.ToString();
        }
    }
}
