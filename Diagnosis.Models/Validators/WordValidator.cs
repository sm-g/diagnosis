using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Common;

namespace Diagnosis.Models.Validators
{
    public class WordValidator : AbstractValidator<Word>
    {
        static char[] forbidden = { '\n', '\t', '\r' };
        public WordValidator()
        {
            RuleFor(w => w.Title).Length(1, 100);
        }
    }
}
