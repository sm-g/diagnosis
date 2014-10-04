using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class Uom : EntityBase, IDomainEntity
    {
        public virtual string Abbr { get; protected set; }
        public virtual string Description { get; set; }
        /// <summary>
        /// Показатель степени множителя по основанию 10 относительно
        /// базовой единицы этого типа (единицы объема: -3 для мкл, 0 для мл).
        /// При сохранении в БД Measure.Value = Value * 10^Factor.
        /// </summary>
        public virtual float Factor { get; set; }
        public virtual int Type { get; set; }

        public Uom(string abbr, int factor, int type)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(abbr));

            this.Abbr = abbr;
            this.Factor = factor;
            this.Type = type;
        }

        protected Uom() { }

        public override string ToString()
        {
            return string.Format("{0}", Abbr);
        }

    }
}
