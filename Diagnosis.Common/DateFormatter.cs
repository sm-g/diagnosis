using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Core
{
    public static class DateFormatter
    {
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
