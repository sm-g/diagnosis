﻿using Diagnosis.Common;
using FluentValidation;
using System;
using System.Linq;

namespace Diagnosis.Models.Validators
{
    public class UomValidator : AbstractValidator<Uom>
    {
        public static string[] TestExistingFor = { "Description", "Abbr", "Type" };

        public UomValidator()
        {
            RuleFor(w => w.Description).NotNull().Length(1, 100);
            RuleFor(w => w.Abbr).NotNull().Length(1, 20);
            RuleFor(w => w.Type).NotNull();
        }
    }
}