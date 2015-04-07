using Diagnosis.Common;
using FluentValidation;
using System;
using System.Linq;

namespace Diagnosis.Models.Validators
{
    public class CourseValidator : AbstractValidator<Course>
    {
        public CourseValidator()
        {
            Func<Course, DateTime> firstAppDate = (x) =>
            {
                var first = x.GetOrderedAppointments().FirstOrDefault();
                if (first != null)
                    return first.DateAndTime.Date;
                else
                    return DateTime.MaxValue;
            };
            Func<Course, DateTime> lastAppDate = (x) =>
            {
                var last = x.GetOrderedAppointments().LastOrDefault();
                if (last != null)
                    return last.DateAndTime.Date;
                else
                    return DateTime.MinValue;
            };

            // курс начинается не позже даты первого осмотра
            RuleFor(p => p.Start).LessThanOrEqualTo(x => firstAppDate(x));
            RuleFor(p => p.Start).LessThanOrEqualTo(x => x.End).When(x => x.IsEnded);

            // курс кончается не раньше даты последнего осмотра
            RuleFor(p => p.End).GreaterThanOrEqualTo(x => lastAppDate(x));
            RuleFor(p => p.End).GreaterThanOrEqualTo(x => x.Start);

            // no neeed to check nullable - http://www.jeremyskinner.co.uk/2011/04/29/fluentvalidation-v3-better-handling-of-nullable-types/
        }
    }
}