using Diagnosis.Common;
using Iesi.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    [DebuggerDisplay("uomtype {Title} {Ord}")]
    [Serializable]
    public class UomType : EntityBase<Guid>, IDomainObject, IComparable<UomType>
    {
        private Iesi.Collections.Generic.ISet<Uom> uoms = new HashedSet<Uom>();
        string _title;

        public UomType(string title, int ord)
        {
            Contract.Requires(title != null);
            Title = title;
            Ord = ord;
        }

        protected UomType()
        {
        }

        /// <summary>
        /// Порядок, уникальный.
        /// </summary>
        public virtual int Ord { get; set; }

        public virtual string Title
        {
            get { return _title; }
            set
            {
                SetProperty(ref _title, value, () => Title);
            }
        }

        /// <summary>
        /// Базовая единица типа (фактор = 0).
        /// Если несколько единиц с фактором 0 (отличаются только названием), базовая первая.
        /// </summary>
        public virtual Uom Base { get { return uoms.FirstOrDefault(x => x.IsBase); } }

        /// <summary>
        /// Единицы типа по увеличению фактора.
        /// </summary>
        public virtual IEnumerable<Uom> Uoms { get { return uoms.OrderBy(x => x.Factor); } }

        /// <summary>
        /// Меняет базу для всех единиц типа.
        /// мл -3 -> мл 0
        /// л   0    л  3
        /// </summary>
        /// <param name="newBase"></param>
        public virtual bool Rebase(Uom newBase)
        {
            if (Base == newBase)
                return false;

            var factor = newBase.Factor;
            uoms.ForAll(u => u.Factor -= factor);
            newBase.Factor = 0;
            OnPropertyChanged(() => Base);
            return true;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Title, Base == null ? "базовая не указана" : Base.Abbr);
        }

        public virtual int CompareTo(UomType other)
        {
            return this.Ord.CompareTo(other.Ord);
        }
    }
}