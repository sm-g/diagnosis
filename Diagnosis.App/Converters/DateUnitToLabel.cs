using Diagnosis.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Diagnosis.App.Converters
{
    public class DateOffsetToUnitLabel : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateOffset))
                return null;

            return (value as DateOffset).GetUnitString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new object[] { value };
        }
    }
    public class OffsetUnitToUnitLabel : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] is DateUnits) || !(values[1] is int?))
                return null;
            var unit = (DateUnits)values[0];
            int? offset = (int?)values[1];

            return DateOffset.FormatUnit(offset, unit);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { value };
        }
    }
}
