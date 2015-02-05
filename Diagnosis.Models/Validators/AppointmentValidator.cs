using Diagnosis.Common;
using FluentValidation;
using System;
using System.Linq;

namespace Diagnosis.Models.Validators
{
    public class AppointmentValidator : AbstractValidator<Appointment>
    {
        public AppointmentValidator()
        {
            // дата осмотра между началом и концом курса
            RuleFor(p => p.DateAndTime).LessThanOrEqualTo(x => x.Course.End).When(x => x.Course.IsEnded);
            RuleFor(p => p.DateAndTime).GreaterThanOrEqualTo(x => x.Course.Start);
        }
    }
}