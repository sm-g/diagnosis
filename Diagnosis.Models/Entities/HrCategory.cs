using System;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    [Serializable]
    public class HrCategory : EntityBase<Guid>, IDomainObject, IComparable, IEquatable<HrCategory>
    {
        public readonly static HrCategory Null = new NullHrCategory();

        [NonSerialized]
        private int _ord;

        public HrCategory(string title, int ord)
        {
            Contract.Requires(title != null);
            Title = title;
            Ord = ord;
        }

        protected HrCategory()
        {
        }

        public virtual string Title { get; set; }

        /// <summary>
        /// Порядок, уникальный.
        /// </summary>
        public virtual int Ord
        {
            get { return _ord; }
            set { _ord = value; }
        }

        public override string ToString()
        {
            return Title;
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

        public virtual bool Equals(HrCategory other)
        {
            if (ReferenceEquals(this, Null) && other == null)
                return true;

            return Equals(other as object);
        }

        private sealed class NullHrCategory : HrCategory
        {
            public NullHrCategory()
                : base("Не задано", int.MaxValue)
            {
            }
        }
    }
}