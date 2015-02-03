using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Common;

namespace Diagnosis.Models.Validators
{
    public class SpecialityValidator : AbstractValidator<Speciality>
    {
        public SpecialityValidator()
        {
            RuleFor(w => w.Title).Length(1, 50);
        }
    }
}
