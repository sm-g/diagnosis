using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Diagnosis.Common;
using System.Windows;

namespace Diagnosis.App.Converters
{
    public class DateIntervalLabel : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (values.Length == 0)
                return null;

            DateTime? from = (DateTime?)(values[0] is DateTime? ? values[0] : null);
            DateTime? to = (DateTime?)(values.Length > 1 && values[1] is DateTime? ? values[1] : null);

            if (from == null && to == null)
                return null;

            var labels = parameter as IList<string>;
            return DateFormatter.GetIntervalString(from, to, labels);
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class DateLabel : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
                return null;

            DateTime from = (DateTime)value;

            var prefix = parameter as string;
            var str = DateFormatter.GetDateString(from);

            if (prefix == null)
                return str;
            else
                return string.Format("{0} {1}", prefix, str);

        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class TimeSpanLabel : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (!(value is TimeSpan))
                return null;

            // префикс, если в конце пробел: 'через '
            var isPrefix = false;
            var label = parameter as string;
            if (label != null && label.Length > 0)
            {
                isPrefix = label.Last() == ' ';
                label = label.Trim();
            }

            TimeSpan ts = (TimeSpan)value;
            var sameTime = "в то же время";
            var str = TimeSpanFormatter.GetTimeSpanString(ts, 3, sameTime);

            if (label == null || str == sameTime)
                return str;
            else
                if (isPrefix)
                    return string.Format("{0} {1}", label, str);
                else
                    return string.Format("{0} {1}", str, label);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
