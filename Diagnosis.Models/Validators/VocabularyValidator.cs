using Diagnosis.Common;
using FluentValidation;
using System;
using System.Linq;

namespace Diagnosis.Models.Validators
{
    public class VocabularyValidator : AbstractValidator<Vocabulary>
    {
        public VocabularyValidator()
        {
            RuleFor(w => w.Title).Length(1, 50);
        }
    }
}