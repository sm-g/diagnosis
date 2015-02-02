using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Diagnosis.Common.Converters
{
    public class MonthToString : BaseValueConverter
    {
        static string ThirteenMonth = DateTimeFormatInfo.CurrentInfo.MonthNames.ToArray().Last();

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                int month = (int)value;

                if (month > 0 && month < 13)
                {
                    return DateTimeFormatInfo.CurrentInfo.GetMonthName(month);
                }
            }
            return ThirteenMonth;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = (string)value;
            if (value == null || name == ThirteenMonth)
            {
                return null;
            }
            return DateTimeFormatInfo.CurrentInfo.MonthNames.ToList().IndexOf(name) + 1;
        }
    }
}
