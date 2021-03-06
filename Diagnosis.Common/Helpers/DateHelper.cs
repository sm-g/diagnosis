﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
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
                if (year.HasValue && year.Value > 0 && year.Value < 10000)
                {
                    y = year.Value;
                }
                var daysInMonth = DateTime.DaysInMonth(y, month.Value);
                if (day.Value > daysInMonth)
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
        /// <summary>
        /// Возвращает возраст к указанному моменту.
        /// </summary>

        [Pure]
        public static int? GetAge(int? birthYear, int? month, int? day, DateTime now)
        {
            if (!birthYear.HasValue)
                return null;

            int age = now.Year - birthYear.Value;
            if (new DateTime(birthYear.Value, month ?? 1, day ?? 1) > now.AddYears(-age))
                age--;

            return age;
        }

        /// <summary>
        /// Возвращает год рождения для указанного возраста к моменту.
        /// </summary>
        [Pure]
        public static int GetBirthYearByAge(int age, int? birthMonth, int? day, DateTime now)
        {
            int year = now.Year - age;
            if (birthMonth.HasValue && day.HasValue)
            {
                DateHelper.CheckAndCorrectDate((int?)year, ref birthMonth, ref day);
                if (new DateTime(year, birthMonth.Value, day.Value) > now.Date.AddYears(-age))
                    year--;
            }
            return year;
        }

        /// <summary>
        /// Возвращает момент когда наступит возраст по дате рождения.
        /// </summary>
        [Pure]
        public static DateTime GetDateForAge(int age, int birthYear, int? month, int? day)
        {
            int year = birthYear + age;

            return new DateTime(year, month ?? 1, day ?? 1);
        }

        /// <summary>
        /// Возвращает год, который должен быть у момента, чтобы в этот момент был указанный возраст.
        /// </summary>
        [Pure]
        public static int GetYearForAge(int age, int birthYear, int? month, int? day, DateTime now)
        {
            var year = birthYear + age;
            var newAge = DateHelper.GetAge(birthYear, month, day, new DateTime(year, now.Month, now.Day));
            if (newAge < age)
                year++;
            return year;
        }

        /// <summary>
        /// 31 и 1 другого месяца отличаются на месяц
        /// from http://stackoverflow.com/a/4639057/3009578
        /// </summary>
        public static int GetTotalMonthsBetween(DateTime now, int year, int month)
        {
            return (now.Year - year) * 12 + now.Month - month;
        }
    }
}
