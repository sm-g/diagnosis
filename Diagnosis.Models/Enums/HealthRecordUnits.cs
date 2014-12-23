using Diagnosis.Common;
using System;

namespace Diagnosis.Models
{
    public enum HealthRecordUnits
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
        public static DateUnits? ToDateOffsetUnit(this HealthRecordUnits unit)
        {
            switch (unit)
            {
                case HealthRecordUnits.Day:
                    return DateUnits.Day;

                case HealthRecordUnits.Week:
                    return DateUnits.Week;

                case HealthRecordUnits.Month:
                    return DateUnits.Month;

                case HealthRecordUnits.Year:
                    return DateUnits.Year;

                default:
                    return null;
            }
        }

        public static HealthRecordUnits ToHealthRecordUnit(this DateUnits unit)
        {
            switch (unit)
            {
                case DateUnits.Day:
                    return HealthRecordUnits.Day;

                case DateUnits.Week:
                    return HealthRecordUnits.Week;

                case DateUnits.Month:
                    return HealthRecordUnits.Month;

                case DateUnits.Year:
                    return HealthRecordUnits.Year;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}