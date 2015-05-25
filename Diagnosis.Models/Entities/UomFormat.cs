using Diagnosis.Common;
using Diagnosis.Models.Validators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    [DebuggerDisplay("uomformat {String} {MeasureValue}")]
    [Serializable]
    public class UomFormat : ValidatableEntity<Guid>, IDomainObject
    {
        private string _str;
        private double _value;
        [NonSerialized] // нужно при создании
        private Uom _uom;

        public UomFormat(string str, double value, Uom uom)
        {
            Contract.Requires(!String.IsNullOrEmpty(str));
            String = str;
            MeasureValue = value;
            Uom = uom;
        }
        public UomFormat()
        {
        }

        public virtual Uom Uom
        {
            get { return _uom; }
            set
            {
                SetProperty(ref _uom, value, () => Uom);
            }
        }
        public virtual string String
        {
            get { return _str; }
            set
            {
                SetProperty(ref _str, value.Truncate(Length.UomFormat), () => String);
            }
        }
        public virtual double MeasureValue
        {
            get { return _value; }
            set
            {
                SetProperty(ref _value, value, () => MeasureValue);
            }
        }


        public override string ToString()
        {
            return string.Format("{0} ({1})", String, MeasureValue);
        }


        public override FluentValidation.Results.ValidationResult SelfValidate()
        {
            return new UomFormatValidator().Validate(this);
        }
    }
}