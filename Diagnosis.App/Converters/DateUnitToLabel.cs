using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Diagnosis.App.Converters
{
    public class DateUnitToLabel : IMultiValueConverter
    {
        static string[] days = new string[3] { "день", "дня", "дней" };
        static string[] months = new string[3] { "месяц", "месяца", "месяцев" };
        static string[] years = new string[3] { "год", "года", "лет" };


        public object Convert(object[] values, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] is DateUnits) || values[1] == null)
                return null;

            var unit = (DateUnits)values[0];
            int offset = (int)values[1];

            if (offset % 10 == 0 || offset % 10 >= 5 || (offset >= 11 && offset <= 14))
            {
                switch (unit)
                {
                    case DateUnits.Day: return days[2];
                    case DateUnits.Month: return months[2];
                    case DateUnits.Year: return years[2];
                }
            }
            if (offset % 10 == 1)
            {
                switch (unit)
                {
                    case DateUnits.Day: return days[0];
                    case DateUnits.Month: return months[0];
                    case DateUnits.Year: return years[0];
                }
            }
            switch (unit)
            {
                case DateUnits.Day: return days[1];
                case DateUnits.Month: return months[1];
                case DateUnits.Year: return years[1];
            }

            throw new ArgumentOutOfRangeException("value");
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, CultureInfo culture)
        {
            return new object[] { value };
            var val = (string)value;

            if (days.Contains(val))
                return new object[] { DateUnits.Day };
            if (months.Contains(val))
                return new object[] { DateUnits.Month };
            if (years.Contains(val))
                return new object[] { DateUnits.Year };

            throw new ArgumentOutOfRangeException("value");
        }
    }
}
