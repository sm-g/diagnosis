using Diagnosis.Core;
using System;

namespace Diagnosis.Models
{
    public enum HealthRecordUnits
    {
        NotSet,
        Day,
        Week,
        Month,
        Year,
    }

    public static class UnitConverter
    {
        public static DateUnits? ToDateOffsetUnit(this HealthRecordUnits unit)
        {
            switch (unit)
            {
                case HealthRecordUnits.NotSet:
                    return null;

                case HealthRecordUnits.Day:
                    return DateUnits.Day;

                case HealthRecordUnits.Week:
                    return DateUnits.Week;

                case HealthRecordUnits.Month:
                    return DateUnits.Month;

                case HealthRecordUnits.Year:
                    return DateUnits.Year;

                default:
                    throw new ArgumentOutOfRangeException();
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