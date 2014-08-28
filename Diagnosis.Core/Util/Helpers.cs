using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Core
{
    public static class DateHelper
    {
        /// <summary>
        /// Проверяет, допустима ли указанная дата (возможно, неполная).
        /// <exception cref="ArgumentOutOfRange"/>
        /// </summary>
        public static void CheckDate(int? year, int? month, int? day)
        {
            if (month.HasValue)
                if (!(month >= 1 && month <= 12))
                    throw new ArgumentOutOfRangeException("month");

            if (day.HasValue)
                if (!(day >= 1 && day <= 31))
                    throw new ArgumentOutOfRangeException("day");

            if (month.HasValue && day.HasValue)
            {
                if (year.HasValue)
                {
                    // пробуем создать дату
                    new DateTime(year.Value, month.Value, day.Value);
                }
                else if (day.Value > DateTime.DaysInMonth(DateTime.Today.Year, month.Value))
                {
                    throw new ArgumentOutOfRangeException("day");
                }
            }
        }

        /// <summary>
        /// Возвращает DateTime, если возможно для указанных аргументов.
        /// </summary>
        public static DateTime? NullableDate(int? year, int? month, int? day)
        {
            try
            {
                return new DateTime(year.Value, month.Value, day.Value);
            }
            catch
            {
                return null;
            }
        }
    }
}
