using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Common;

namespace Diagnosis.Models.Validators
{
    public class DoctorValidator : AbstractValidator<Doctor>
    {
        public DoctorValidator()
        {
            RuleFor(p => p.LastName).NotNull().Length(1, 20);
            RuleFor(p => p.MiddleName).Length(0, 20);
            RuleFor(p => p.FirstName).Length(0, 20);
        }
    }
}
