using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System;

namespace Diagnosis.Models
{
    public class HrCategory : EntityBase, IDomainObject, IComparable
    {
        public virtual string Name { get; set; }
        public virtual int Ord { get; set; }

        public override string ToString()
        {
            return Name;
        }
        public virtual int CompareTo(object obj)
        {
            if (obj == null)
                return -1;

            HrCategory other = obj as HrCategory;
            if (other != null)
            {
                return this.Ord.CompareTo(other.Ord);
            }
            else
                throw new ArgumentException("Object is not a Category");
        }
    }
}
