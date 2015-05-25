using Diagnosis.Common;
using FluentValidation;
using System;
using System.Linq;

namespace Diagnosis.Models.Validators
{
    public class DoctorValidator : AbstractValidator<Doctor>
    {
        public DoctorValidator()
        {
            RuleFor(p => p.LastName).NotNull().Length(1, Length.DoctorLn);
            RuleFor(p => p.MiddleName).Length(0, Length.DoctorMn);
            RuleFor(p => p.FirstName).Length(0, Length.DoctorFn);
        }
    }
}