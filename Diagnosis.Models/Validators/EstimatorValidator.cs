﻿using Diagnosis.Common;
using FluentValidation;
using System;
using System.Linq;

namespace Diagnosis.Models.Validators
{
    public class EstimatorValidator : AbstractValidator<Estimator>
    {
        public EstimatorValidator()
        {
            RuleFor(w => w.Description).Length(1, 2000).NotNull();
            //    RuleFor(w => w.HeaderHrsOptions).NotEmpty();
        }
    }
}