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
        /// Если год не указан, считаем 29 февраля для сегодняшего года.
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
        /// Проверяет, допустима ли указанная дата (возможно, неполная). 
        /// Если недопустима, исправляет месяц на 12 и день на последний в месяце.
        /// Если год не указан, считаем 29 февраля для сегодняшего года.
        /// </summary>
        /// <returns>
        /// True, если было исправление.
        /// </returns>
        public static bool CheckAndCorrectDate(int? year, ref int? month, ref int? day)
        {
            bool corrected = false;
            if (month.HasValue && (month < 1 || month > 12))
            {
                month = 12;
                corrected = true;
            }

            if (!month.HasValue && day.HasValue && (day < 1 || day > 31))
            {
                day = 31;
                corrected = true;
            }

            if (month.HasValue && day.HasValue)
            {
                int y = DateTime.Today.Year;
                if (year.HasValue)
                {
                    y = year.Value;
                }
                var daysInMonth = DateTime.DaysInMonth(y, month.Value);
                if (day.Value > daysInMonth || day.Value < daysInMonth)
                {
                    day = daysInMonth;
                    corrected = true;
                }
            }
            return corrected;
        }
        /// <summary>
        /// Проверяет, допустима ли указанная дата (возможно, неполная). 
        /// Если недопустима, исправляет месяц на 12 и день на последний в месяце.
        /// Если год не указан, считаем 29 февраля для сегодняшего года.
        /// </summary>
        /// <returns>
        /// True, если было исправление.
        /// </returns>
        public static bool CheckAndCorrectDate(int? year, ref byte? month, ref byte? day)
        {
            int? m = month;
            int? d = day;
            if (CheckAndCorrectDate(year, ref m, ref d))
            {
                month = (byte?)m;
                day = (byte?)d;
                return true;
            }
            return false;
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
