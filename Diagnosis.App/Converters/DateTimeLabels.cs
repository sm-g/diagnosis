using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows;

namespace Diagnosis.App.Converters
{
    public class DateIntervalLabel : BaseMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0)
                return DependencyProperty.UnsetValue;

            DateTime? from = (DateTime?)(values[0] is DateTime? ? values[0] : null);
            DateTime? to = (DateTime?)(values.Length > 1 && values[1] is DateTime? ? values[1] : null);

            if (from == null && to == null)
                return "";

            var labels = parameter as IList<string>;
            return DateFormatter.GetIntervalString(from, to, labels);
        }
    }
    /// <summary>
    /// Дата или дата-время относительно сейчас, если есть параметр.
    /// </summary>
    public class DateLabel : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
                return DependencyProperty.UnsetValue;

            DateTime from = (DateTime)value;

            var relativeEnding = parameter as string;

            if (relativeEnding == null)
                return DateFormatter.GetDateString(from);
            else
                return DateFormatter.GetRelativeDateString(from, relativeEnding);
        }

    }
    /// <summary>
    /// Промежуток времени (использовать с параметром, 'через ' или ' ранее').
    /// </summary>
    public class TimeSpanLabel : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TimeSpan))
                return DependencyProperty.UnsetValue;

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
    }
}