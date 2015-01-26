using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{
    public static class EnumExtensions
    {
        public static DateUnit GetNextDateUnit(this DateUnit unit)
        {
            switch (unit)
            {
                case DateUnit.Day:
                    return DateUnit.Week;

                case DateUnit.Week:
                    return DateUnit.Month;

                case DateUnit.Month:
                    return DateUnit.Year;

                case DateUnit.Year:
                    return DateUnit.Day;

            }
            throw new NotImplementedException();
        }
    }
}
