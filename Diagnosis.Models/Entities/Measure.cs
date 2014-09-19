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
    public class Measure : EntityBase, IDomainEntity
    {
        public virtual HealthRecord HealthRecord { get; protected set; }
        public virtual Uom Uom { get; protected set; }
        /// <summary>
        /// Значение измерения, приведенное к базовой единице измерения.
        /// </summary>
        public virtual float DbValue { get; protected set; }
        /// <summary>
        /// Значение измерения.
        /// </summary>
        public virtual float Value
        {
            get
            {
                return DbValue * (float)Math.Pow(10, -Uom.Factor);
            }
            protected set
            {
                DbValue = value * (float)Math.Pow(10, Uom.Factor);
            }
        }
        public Measure(float value, Uom uom, HealthRecord hr)
        {
            Contract.Requires(uom != null);
            Contract.Requires(hr != null);
            Uom = uom;
            HealthRecord = hr;
            Value = value;
        }

        protected Measure() { }

        public override string ToString()
        {
            return string.Format("{0} {1}", Value, Uom);
        }

    }
}
