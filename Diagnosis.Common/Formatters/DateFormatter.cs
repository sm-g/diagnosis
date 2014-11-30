using System;
using System.Collections.Generic;
using System.Text;

namespace Diagnosis.Common
{
    public static class DateFormatter
    {
        private static string[] days = new string[3] { "день", "дня", "дней" };

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

        public static string GetDateString(DateTime date)
        {
            var formats = DateFormatter.GetFormat(date, null);
            return date.ToString(formats.Item1);
        }

        /// <summary>
        /// 
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
                sb.Append(days[i]);
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
}