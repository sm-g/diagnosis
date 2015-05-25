using Diagnosis.Common;
using FluentValidation;
using System;
using System.Linq;

namespace Diagnosis.Models.Validators
{
    public class UomValidator : AbstractValidator<Uom>
    {
        public UomValidator()
        {
            RuleFor(w => w.Description).NotNull().Length(1, Length.UomDescr);
            RuleFor(w => w.Abbr).NotNull().Length(1, Length.UomAbbr);
            RuleFor(w => w.Type).NotNull();
        }
    }
}