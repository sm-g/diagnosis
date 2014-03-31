using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Diagnosis.App.Converters
{
    public class MonthToString : IValueConverter
    {
        static string ThirteenMonth = DateTimeFormatInfo.CurrentInfo.MonthNames.ToArray().Last();

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            int month = (int)value;

            if (month > 0 && month < 13)
            {
                return DateTimeFormatInfo.CurrentInfo.GetMonthName(month);
            }
            return ThirteenMonth;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string name = (string)value;
            if (value == null || name == ThirteenMonth)
            {
                return 13;
            }
            return DateTimeFormatInfo.CurrentInfo.MonthNames.ToList().IndexOf(name) + 1;
        }

        public MonthToString() { }
    }
}
