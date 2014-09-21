using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models.Validators
{
    public class PatientValidator : AbstractValidator<Patient>
    {
        public PatientValidator()
        {
            RuleFor(pat => pat.Label).NotEmpty().WithMessage("Метка должна быть."); // только если нет ФИО?
        }
    }
}
