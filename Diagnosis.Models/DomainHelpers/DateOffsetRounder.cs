using Diagnosis.Common;
using System;
using System.Diagnostics.Contracts;

using System.Linq;

namespace Diagnosis.Models
{
    public static class DateOffsetRounder
    {
        /// <summary>
        /// Округляет смещение.
        /// При укрупнении единицы смещение считается для полной даты с 1 вместо отсутствующих значений.
        /// </summary>
        public static int? RoundOffsetFor(this DateOffset d, DateUnit unit)
        {
            Contract.Requires(d != null);
            Contract.Ensures(d.Equals(Contract.OldValue(d)));
            Contract.Ensures(!d.IsEmpty || Contract.Result<int?>() == null);

            if (!d.Year.HasValue)
            {
                return null;
            }
            int? roundedOffset;
            switch (unit)
            {
                case DateUnit.Day:
                    roundedOffset = (d.Now - d.GetSortingDate()).Days;
                    break;

                case DateUnit.Week:
                    roundedOffset = (d.Now - d.GetSortingDate()).Days / 7;
                    break;

                case DateUnit.Month:
                    if (d.Month.HasValue)
                    {
                        roundedOffset = DateHelper.GetTotalMonthsBetween(d.Now, d.Year.Value, d.Month.Value);
                    }
                    else
                    {
                        roundedOffset = DateHelper.GetTotalMonthsBetween(d.Now, d.Year.Value, 1);
                    }
                    break;

                case DateUnit.Year:
                    roundedOffset = d.Now.Year - d.Year.Value;
                    break;

                default:
                    throw new NotImplementedException();
            }
            return roundedOffset;
        }

        /// <summary>
        /// Установка даты меняет единицу измерения и смещение на наиболее подходящие.
        /// </summary>
        public static DateOffset RoundOffsetUnitByDate(this DateOffset d, DateTime described)
        {
            Contract.Requires(d.Year != null);
            Contract.Ensures(d.Equals(Contract.OldValue(d)));

            int? offset = null;
            DateUnit unit = 0;

            Action setRoundedOffsetUnitMonthOrYear = () =>
            {
                var months = DateHelper.GetTotalMonthsBetween(described, d.Year.Value, d.Month.Value);
                if (months < 12) // меньше года - месяцы
                {
                    offset = months;
                    unit = DateUnit.Month;
                }
                else
                {
                    offset = described.Year - d.Year.Value;
                    unit = DateUnit.Year;
                }
            };

            if (d.Month == null) // _ _ y (или d _ y без автообрезания)
            {
                offset = described.Year - d.Year.Value;
                unit = DateUnit.Year;
            }
            else if (d.Day == null) // _ m y
            {
                setRoundedOffsetUnitMonthOrYear();
            }
            else // d m y
            {
                var days = (described - (DateTime)d).Days;
                if (days < 7) // меньше недели - дни
                {
                    offset = days;
                    unit = DateUnit.Day;
                }
                else if (days < 4 * 7) // меньше месяца - недели
                {
                    offset = days / 7;
                    unit = DateUnit.Week;
                }
                else
                {
                    setRoundedOffsetUnitMonthOrYear();
                }
            }

            return new DateOffset(offset, unit, () => d.Now);
        }
    }
}