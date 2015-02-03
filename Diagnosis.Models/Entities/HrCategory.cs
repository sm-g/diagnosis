using System;

namespace Diagnosis.Models
{
    public class HrCategory : EntityBase<int>, IDomainObject, IComparable, IEquatable<HrCategory>
    {
        public static HrCategory Null = new HrCategory() { Name = "Не задано", Ord = int.MaxValue }; // upper case to show in checkbox

        public virtual string Name { get; set; }
        /// <summary>
        /// Порядок, уникальный.
        /// </summary>
        public virtual int Ord { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public virtual int CompareTo(object obj)
        {
            if (obj == null)
                return object.ReferenceEquals(this, Null) ? 0 : -1;

            HrCategory other = obj as HrCategory;
            if (other != null)
                return this.Ord.CompareTo(other.Ord);
            else
                throw new ArgumentException("Object is not a HrCategory");
        }

        public static bool operator ==(HrCategory x, HrCategory y)
        {
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
                return object.ReferenceEquals(x, y);

            return x.Equals(y);
        }

        public static bool operator !=(HrCategory x, HrCategory y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
            // Null-value равен null
            if (object.ReferenceEquals(obj, null))
                return object.ReferenceEquals(this, Null);

            var other = obj as HrCategory;
            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public virtual bool Equals(HrCategory other)
        {
            if (object.ReferenceEquals(other, null))
                return object.ReferenceEquals(this, Null);
            else if (object.ReferenceEquals(this, Null) || object.ReferenceEquals(other, Null))
                // Null-value не равен катгории с таким же именем
                return object.ReferenceEquals(this, other);

            return this.Name == other.Name;
        }
    }
}