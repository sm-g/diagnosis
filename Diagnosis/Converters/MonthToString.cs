using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Diagnosis.Converters
{
    public class MonthToString : IValueConverter
    {
        string ThirteenMonth = "";

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            int month = (int)value;
            if (month > 11)
            {
                return ThirteenMonth;
            }

            return DateTimeFormatInfo.CurrentInfo.GetMonthName(month + 1);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string name = (string)value;
            if (name == ThirteenMonth)
            {
                return 13;
            }
            var q = DateTimeFormatInfo.CurrentInfo.MonthNames.ToList();
            return DateTimeFormatInfo.CurrentInfo.MonthNames.ToList().IndexOf(name) + 1;
        }

        public MonthToString() { }

        public MonthToString(string thirteenMonth)
        {
            ThirteenMonth = thirteenMonth;
        }
    }
}
