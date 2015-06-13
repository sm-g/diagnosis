using Diagnosis.Common;
using System;
using System.Linq;

namespace Diagnosis.Models
{
    public static class DateOffsetFormatter
    {
        /// <summary>
        /// Unit of DateOffset with ending for given offset.
        /// </summary>
        public static string GetUnitString(int? offset, DateUnit unit)
        {
            if (offset == null)
                offset = 0;

            int ending = PluralsHelper.GetPluralEnding(offset.Value);

            switch (unit)
            {
                case DateUnit.Day: return Plurals.days[ending];
                case DateUnit.Week: return Plurals.weeks[ending];
                case DateUnit.Month: return Plurals.months[ending];
                case DateUnit.Year: return Plurals.years[ending];
            }
            throw new ArgumentOutOfRangeException("unit");
        }

        /// <summary>
        /// DateOffset as partial DateTime, i.e. "2014"
        /// </summary>
        public static string GetPartialDateString(DateOffset d)
        {
            if (d == null || d.IsEmpty || d.Year == null)
                return string.Empty;
            if (!d.Month.HasValue) // year
                return d.Year.ToString();
            if (!d.Day.HasValue) // month year
                return System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames[d.Month.Value - 1].ToLower() + " " + d.Year.ToString();
            return ((DateTime)d).ToString("d MMMM yyyy"); // full
        }

        /// <summary>
        /// DateOffset as Offset with Unit, i.e. "1 день"
        /// </summary>
        public static string GetOffsetUnitString(DateOffset d)
        {
            if (d == null || d.IsEmpty)
                return string.Empty;
            return string.Format("{0} {1}", d.Offset, GetUnitString(d.Offset, d.Unit));
        }
    }
}