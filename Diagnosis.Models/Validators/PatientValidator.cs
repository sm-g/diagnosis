using Diagnosis.Common;
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
            RuleFor(p => p.LastName).Length(0, 20);
            RuleFor(p => p.MiddleName).Length(0, 20);
            RuleFor(p => p.FirstName).Length(0, 20);
            RuleFor(p => p.Age).GreaterThanOrEqualTo(0);

            Custom(p =>
            {
                // не нужно, пока дата вводится через DatePicker
                try
                {
                    DateHelper.CheckDate(p.BirthYear, p.BirthMonth, p.BirthDay);
                    return null;
                }
                catch (Exception)
                {
                    return new ValidationFailure("Date", "Неверная дата рождения.");
                }
            });
        }
    }
}