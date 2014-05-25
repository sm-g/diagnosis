using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Core
{
    static class Checkers
    {
        public const int MinYear = 1880;

        /// <summary>
        /// Проверяет, допустима ли указанная дата (возможно, неполная).
        /// <exception cref="ArgumentOutOfRange">Если дата невозможна.</exception>
        /// </summary>
        public static void CheckDate(int? year, int? month, int? day)
        {
            if (month.HasValue && day.HasValue)
            {
                if (year.HasValue)
                {
                    if (year < MinYear)
                    {
                        throw new ArgumentOutOfRangeException("year");
                    }

                    // пробуем создать дату
                    new DateTime(year.Value, month.Value, day.Value);
                }
                else if (day.Value > DateTime.DaysInMonth(DateTime.Today.Year, month.Value))
                {
                    throw new ArgumentOutOfRangeException("day");
                }
            }
        }

    }
}
