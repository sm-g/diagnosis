using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

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

            var relativeEnding = parameter as string;

            if (relativeEnding == null)
                return DateFormatter.GetDateString(from);
            else
                return DateFormatter.GetRelativeDateString(from, relativeEnding);
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

    public class DateOffsetToUnitLabel : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d = value as DateOffset;
            if (d == null)
                return null;

            return DateOffsetFormatter.GetUnitString(d.Offset, d.Unit);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class DateOffsetToLabel : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d = value as DateOffset;
            if (d == null)
                return null;

            var unit = HealthRecordUnit.NotSet;

            if (parameter is HealthRecordUnit)
                unit = (HealthRecordUnit)parameter;
            switch (unit)
            {
                case HealthRecordUnit.NotSet:
                    return DateOffsetFormatter.GetPartialDateString(d);

                case HealthRecordUnit.ByAge: // by age должен брать дату пациента
                default:
                    return DateOffsetFormatter.GetOffsetUnitString(d);
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Для DateOffsetPicker
    /// </summary>
    public class OffsetUnitToUnitLabel : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] is DateUnit) || !(values[1] is int?))
                return null;
            var unit = (DateUnit)values[0];
            int? offset = (int?)values[1];

            return DateOffsetFormatter.GetUnitString(offset, unit);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { value };
        }
    }
}