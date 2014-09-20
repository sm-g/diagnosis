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
        HealthRecord _hr;
        public virtual HealthRecord HealthRecord
        {
            get
            {
                return _hr;
            }
            set
            {
                if (_hr != null)
                {
                    throw new InvalidOperationException("Can not change HealthRecord associated wiht measure.");
                }
                _hr = value;
            }
        }
        public virtual Uom Uom { get; protected set; }
        /// <summary>
        /// Значение измерения, приведенное к базовой единице измерения.
        /// </summary>
        public virtual float DbValue { get; protected set; }
        /// <summary>
        /// Значение измерения. Если не указана единица, Value == DbValue.
        /// </summary>
        public virtual float Value
        {
            get
            {
                return DbValue * (Uom != null ? (float)Math.Pow(10, -Uom.Factor) : 1);
            }
            protected set
            {
                DbValue = value * (Uom != null ? (float)Math.Pow(10, Uom.Factor) : 1);
            }
        }
        public Measure(float value, Uom uom = null)
        {
            Uom = uom;
            Value = value;
        }
        protected Measure() { }

        public override string ToString()
        {
            return string.Format("{0} {1}", Value, Uom);
        }

    }
}
