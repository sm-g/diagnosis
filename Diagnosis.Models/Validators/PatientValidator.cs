﻿using Diagnosis.Common;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Linq;

namespace Diagnosis.Models.Validators
{
    public class PatientValidator : AbstractValidator<Patient>
    {
        public PatientValidator()
        {
            RuleFor(p => p.LastName).Length(0, Length.PatientLn);
            RuleFor(p => p.MiddleName).Length(0, Length.PatientMn);
            RuleFor(p => p.FirstName).Length(0, Length.PatientFn);
            RuleFor(p => p.Age).GreaterThanOrEqualTo(0);

            Custom(p =>
            {
                try
                {
                    DateHelper.CheckDate(p.BirthYear, p.BirthMonth, p.BirthDay);
                    return null;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return new ValidationFailure("Date", "Неверная дата рождения.");
                }
            });
        }
    }
}