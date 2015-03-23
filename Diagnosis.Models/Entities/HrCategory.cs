using System;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class HrCategory : EntityBase<Guid>, IDomainObject, IComparable//, IEquatable<HrCategory>
    {
        public static HrCategory Null = new HrCategory("Не задано", int.MaxValue); // upper case to show in checkbox

        public HrCategory(string name, int ord)
        {
            Contract.Requires(name != null);
            Name = name;
            Ord = ord;
        }

        protected HrCategory()
        {
        }

        public virtual string Name { get; set; }

        /// <summary>
        /// Порядок, уникальный.
        /// </summary>
        public virtual int Ord { get; set; }

        public static bool ConsideredNull(HrCategory x)
        {
            return x == null || object.Equals(x, HrCategory.Null);
        }

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
                return this.Ord.CompareTo(other.Ord);
            else
                throw new ArgumentException("Object is not a HrCategory");
        }
    }
}