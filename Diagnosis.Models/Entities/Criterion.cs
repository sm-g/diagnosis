using Diagnosis.Common;
using Diagnosis.Models.Validators;
using FluentValidation.Results;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

using System.Linq;

namespace Diagnosis.Models
{
    [DebuggerDisplay("Criterion {Code} {Value}")]
    public class Criterion : CritBase
    {
        private string _code;
        private string _value;
        private CriteriaGroup _group;

        public Criterion(CriteriaGroup gr)
            : base()
        {
            Contract.Requires(gr != null);
            Group = gr;
            Code = "";
            Value = "";
        }

        protected internal Criterion()
        {
        }

        public virtual CriteriaGroup Group
        {
            get { return _group; }
            set { SetProperty(ref _group, value, () => Group); }
        }

        public virtual string Code
        {
            get { return _code; }
            set { SetProperty(ref _code, value.Truncate(Length.CriterionCode), () => Code); }
        }

        public virtual string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value.Truncate(Length.CriterionValue), () => Value); }
        }

        public override string ToString()
        {
            return "{0} {1}".FormatStr(Code, Description.Truncate(20)).Replace(Environment.NewLine, " ");
        }

        public override ValidationResult SelfValidate()
        {
            return new CriterionValidator().Validate(this);
        }
    }
}