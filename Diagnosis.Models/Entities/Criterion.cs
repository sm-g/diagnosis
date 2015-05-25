using Diagnosis.Common;
using Diagnosis.Models.Validators;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    [DebuggerDisplay("Criterion {Code} {Value}")]
    public class Criterion : ValidatableEntity<Guid>, IDomainObject, ICrit
    {
        private string _description;
        private string _code;
        private string _value;
        private string _options;
        private CriteriaGroup _group;

        public Criterion(CriteriaGroup gr)
        {
            Contract.Requires(gr != null);
            Group = gr;
            Description = "";
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

        public virtual string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value.Truncate(Length.CriterionDescr), () => Description); }
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
        public virtual string Options
        {
            get { return _options; }
            set { SetProperty(ref _options, value, () => Options); }
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