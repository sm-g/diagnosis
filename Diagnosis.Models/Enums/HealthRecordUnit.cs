﻿using Diagnosis.Common;
using System;

namespace Diagnosis.Models
{
    public enum HealthRecordUnit
    {
        /// <summary>
        /// Показываем дату
        /// </summary>
        NotSet,

        Day,
        Week,
        Month,
        Year,

        /// <summary>
        /// Показываем, с какого возраста пациента
        /// </summary>
        ByAge
    }

    public static class UnitConverter
    {
        public static DateUnit? ToDateOffsetUnit(this HealthRecordUnit unit)
        {
            switch (unit)
            {
                case HealthRecordUnit.Day:
                    return DateUnit.Day;

                case HealthRecordUnit.Week:
                    return DateUnit.Week;

                case HealthRecordUnit.Month:
                    return DateUnit.Month;

                case HealthRecordUnit.Year:
                    return DateUnit.Year;

                default:
                    return null;
            }
        }

        public static HealthRecordUnit ToHealthRecordUnit(this DateUnit unit)
        {
            switch (unit)
            {
                case DateUnit.Day:
                    return HealthRecordUnit.Day;

                case DateUnit.Week:
                    return HealthRecordUnit.Week;

                case DateUnit.Month:
                    return HealthRecordUnit.Month;

                case DateUnit.Year:
                    return HealthRecordUnit.Year;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}