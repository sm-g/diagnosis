using Diagnosis.Common;
using FluentValidation;
using System;
using System.Linq;

namespace Diagnosis.Models.Validators
{
    public class CriteriaGroupValidator : AbstractValidator<CriteriaGroup>
    {
        public CriteriaGroupValidator()
        {
            RuleFor(w => w.Description).Length(1, 2000);
        }
    }
}