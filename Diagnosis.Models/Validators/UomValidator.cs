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
        public static string[] TestExistingFor = { "Description", "Abbr" };
        public UomValidator()
        {
            RuleFor(w => w.Description).NotNull().Length(1, 100);
            RuleFor(w => w.Abbr).NotNull().Length(1, 10);
            RuleFor(w => w.Type).NotNull();
        }
    }
}
