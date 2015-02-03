using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Iesi.Collections.Generic;
using FluentValidation.Results;
using Diagnosis.Models.Validators;
using Diagnosis.Common;

namespace Diagnosis.Models
{
    [Serializable]

    public class UomType : EntityBase<int>, IDomainObject, IComparable<UomType>
    {
        private Iesi.Collections.Generic.ISet<Uom> uoms = new HashedSet<Uom>();
        public UomType(string title)
        {
            Contract.Requires(title != null);
            Title = title;
        }

        protected UomType() { }

        /// <summary>
        /// Порядок, уникальный.
        /// </summary>
        public virtual int Ord { get; set; }

        public virtual string Title { get; set; }

        public virtual Uom Base { get { return uoms.SingleOrDefault(x => x.IsBase); } }
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
            return true;
        }
        public override string ToString()
        {
            return string.Format("{0}", Title);
        }


        public virtual int CompareTo(UomType other)
        {
            return this.Ord.CompareTo(other.Ord);
        }
    }
}
