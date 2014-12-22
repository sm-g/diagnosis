using System;
using System.Collections.Generic;
using System.Text;

namespace Diagnosis.Common
{
    public static class DateFormatter
    {
        /// <summary>
        /// Интервал дат без дублирования информации.
        /// Например: с 10 по 25 мая 2000
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="labels"></param>
        /// <returns></returns>
        public static string GetIntervalString(DateTime? from, DateTime? to = null, IList<string> labels = null)
        {
            if (from == null && to == null)
                return "";

            var fromLabel = "с";
            var toLabel = "по";
            if (labels != null && labels.Count > 0)
            {
                fromLabel = labels[0];
                if (labels.Count > 1)
                    toLabel = labels[1];
            }

            Tuple<string, string> formats;

            if (from.HasValue)
            {
                formats = DateFormatter.GetFormat(from.Value, to); // to may be null
            }
            else // only to.HasValue
            {
                formats = DateFormatter.GetFormat(to.Value, null);
            }

            if (from.HasValue && to.HasValue)
                if (from == to)
                    return string.Format("{0}", from.Value.ToString(formats.Item1)); // та же дата - интервала нет
                else
                    return string.Format("{0} {1} {2} {3}", fromLabel, from.Value.ToString(formats.Item1), toLabel, to.Value.ToString(formats.Item2));
            else
            {
                if (from.HasValue)
                    return string.Format("{0} {1}", fromLabel, from.Value.ToString(formats.Item1));
                else // to.HasValue
                    return string.Format("{0} {1}", toLabel, to.Value.ToString(formats.Item1));
            }
        }

        /// <summary>
        /// Дата без года, если он равен текущему.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetDateString(DateTime date)
        {
            var formats = DateFormatter.GetFormat(date, null);
            return date.ToString(formats.Item1);
        }

        public static Tuple<string, string> GetFormat(DateTime from, DateTime? to = null)
        {
            DateTime now = DateTime.Today;
            string fromFormat = "d MMMM";
            string toFormat = "d MMMM yyyy";

            // если нет второй даты или даты полностью совпадают,
            // то проверяем только совпадение года первой даты с текущим
            //    формат второй даты неопределен
            Action noTo = () =>
            {
                if (from.Year != now.Year)
                {
                    fromFormat = "d MMMM yyyy";
                }
                toFormat = "";
            };

            if (to.HasValue)
            {
                if (to.Value.Year == from.Year)
                {
                    if (from.Year == now.Year)
                    {
                        toFormat = "d MMMM";
                    }
                    if (to.Value.Month == from.Month)
                    {
                        if (to.Value.Day == from.Day)
                        {
                            noTo();
                        }
                        else
                        {
                            fromFormat = "%d";
                        }
                    }
                }
                else
                {
                    fromFormat = "d MMMM yyyy";
                }
            }
            else
            {
                noTo();
            }

            return new Tuple<string, string>(fromFormat, toFormat);
        }
    }

    public static class TimeSpanFormatter
    {
        /// <summary>
        /// Промежуток времени. При кол-ве дней меньше daysLimit показываются часы и минуты.
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="daysLimit">Через сколько дней показывать часы и минуты</param>
        /// <param name="sameAndNegative"></param>
        /// <returns></returns>
        public static string GetTimeSpanString(TimeSpan ts, int daysLimit, string sameAndNegative = "same")
        {
            if (ts < TimeSpan.Zero || ts.TotalMinutes < 1)
                return sameAndNegative;

            var sb = new StringBuilder();
            var i = Plurals.GetPluralEnding(ts.Days);
            if (ts.Days > 0)
            {
                sb.Append("{0:%d} ");
                sb.Append(Plurals.days[i]);
                if (ts.Days < daysLimit)
                {
                    sb.Append(" ");
                }
            }
            if (ts.Days < daysLimit)
            {
                sb.Append("{0:%h} ч");

                if (ts.Minutes > 0)
                    sb.Append(" {0:%m} м");
            }
            return string.Format(sb.ToString(), ts);
        }
    }

    public static class DateOffsetFormatter
    {
        /// <summary>
        /// Unit of DateOffset with ending for given offset.
        /// </summary>
        public static string FormatUnit(int? offset, DateUnits unit)
        {
            if (offset == null)
                offset = 0;

            int ending = Plurals.GetPluralEnding(offset.Value);

            switch (unit)
            {
                case DateUnits.Day: return Plurals.days[ending];
                case DateUnits.Week: return Plurals.weeks[ending];
                case DateUnits.Month: return Plurals.months[ending];
                case DateUnits.Year: return Plurals.years[ending];
            }
            throw new ArgumentOutOfRangeException("unit");
        }

        /// <summary>
        /// DateOffset as partial DateTime, i.e. "2014"
        /// </summary>
        public static string GetPartialDateString(DateOffset d)
        {
            if (d == null || d.IsEmpty)
                return string.Empty;
            if (!d.Month.HasValue) // year
                return d.Year.ToString();
            if (!d.Day.HasValue) // month year
                return System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames[d.Month.Value - 1].ToLower() + " " + d.Year.ToString();
            return d.GetNullableDateTime().Value.ToString("d MMMM yyyy"); // full
        }
    }
}