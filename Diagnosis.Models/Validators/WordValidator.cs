using Diagnosis.Common;
using FluentValidation;
using System;
using System.Linq;

namespace Diagnosis.Models.Validators
{
    public class WordValidator : AbstractValidator<Word>
    {
        private static char[] forbidden = { '\n', '\t', '\r' };

        public WordValidator()
        {
            RuleFor(w => w.Title).Length(1, 100).NotNull();
        }
    }
}