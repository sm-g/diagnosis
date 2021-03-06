﻿using Diagnosis.Common;
using FluentValidation;
using System;
using System.Linq;

namespace Diagnosis.Models.Validators
{
    public class UomFormatValidator : AbstractValidator<UomFormat>
    {
        public UomFormatValidator()
        {
            RuleFor(x => x.Uom).NotNull();
            RuleFor(w => w.String).NotNull().Length(1, Length.UomFormat);
        }
    }
}