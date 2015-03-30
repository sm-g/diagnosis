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
            RuleFor(p => p.DateAndTime.Date).LessThanOrEqualTo(x => x.Course.End).When(x => x.Course.IsEnded);
            RuleFor(p => p.DateAndTime.Date).GreaterThanOrEqualTo(x => x.Course.Start);
        }
    }
}