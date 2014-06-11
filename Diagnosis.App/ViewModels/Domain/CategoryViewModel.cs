using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Diagnosis.App.Messaging;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class CategoryViewModel : CheckableBase, IComparable
    {
        internal readonly Category category;


        public string Name
        {
            get
            {
                return category.Title;
            }
        }


        public CategoryViewModel(Category category)
        {
            Contract.Requires(category != null);

            this.category = category;
        }

        public override void OnCheckedChanged()
        {
            this.Send((int)EventID.CategoryCheckedChanged, new CategoryParams(this).Params);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return -1;

            CategoryViewModel other = obj as CategoryViewModel;
            if (other != null)
            {
                return this.category.Order.CompareTo(other.category.Order);
            }
            else
                throw new ArgumentException("Object is not a CategoryViewModel");
        }

        public override string ToString()
        {
            return category.ToString();
        }
    }
}