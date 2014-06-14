using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System;

namespace Diagnosis.Models
{
    public class Category : IEntity, IComparable
    {
        public virtual int Id { get; protected set; }
        public virtual string Name { get; set; }
        public virtual int Order { get; set; }

        public override string ToString()
        {
            return Name;
        }
        public virtual int CompareTo(object obj)
        {
            if (obj == null)
                return -1;

            Category other = obj as Category;
            if (other != null)
            {
                return this.Order.CompareTo(other.Order);
            }
            else
                throw new ArgumentException("Object is not a CategoryViewModel");
        }
    }
}
