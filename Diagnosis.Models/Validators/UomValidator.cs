using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Common;

namespace Diagnosis.Models.Validators
{
    public class UomValidator : AbstractValidator<Uom>
    {
        public UomValidator()
        {
            RuleFor(w => w.Description).Length(1, 100);
            RuleFor(w => w.Abbr).NotNull().Length(1, 10);
        }
    }
}
