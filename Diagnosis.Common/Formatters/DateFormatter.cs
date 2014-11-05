using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{
    public static class DateFormatter
    {
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

        public static Tuple<string, string> GetFormat(DateTime from, DateTime? to = null)
        {
            DateTime now = DateTime.Today;
            string fromFormat = "d MMMM";
            string toFormat = "d MMMM yyyy";

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
                            if (from.Year != now.Year)
                            {
                                fromFormat = "d MMMM yyyy";
                            }
                            toFormat = "";
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
                toFormat = "";
                if (from.Year != now.Year)
                {
                    fromFormat = "d MMMM yyyy";
                }
            }

            return new Tuple<string, string>(fromFormat, toFormat);
        }
    }
}
