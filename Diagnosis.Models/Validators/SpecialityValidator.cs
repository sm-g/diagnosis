using Diagnosis.Common;
using FluentValidation;
using System;
using System.Linq;

namespace Diagnosis.Models.Validators
{
    public class SpecialityValidator : AbstractValidator<Speciality>
    {
        public SpecialityValidator()
        {
            RuleFor(w => w.Title).Length(1, 50).NotNull();
        }
    }
}