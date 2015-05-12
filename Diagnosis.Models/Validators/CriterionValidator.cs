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
            RuleFor(w => w.Description).Length(1, 2000).NotNull();
            RuleFor(w => w.Value).Length(1, 50).NotNull();
            RuleFor(w => w.Code).Length(1, 50).NotNull();
            RuleFor(w => w.Group).NotEmpty();
            //    RuleFor(w => w.Options).NotEmpty();
        }
    }
}