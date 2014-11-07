using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Common;

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
