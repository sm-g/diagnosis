using Diagnosis.Common;
using FluentValidation;
using System;
using System.Linq;

namespace Diagnosis.Models.Validators
{
    public class CriterionValidator : AbstractValidator<Criterion>
    {
        public CriterionValidator()
        {
            RuleFor(w => w.Description).Length(1, 2000);
            RuleFor(w => w.Value).Length(1, 50);
            RuleFor(w => w.Code).Length(1, 50);
            RuleFor(w => w.Options).NotEmpty();
        }
    }
}