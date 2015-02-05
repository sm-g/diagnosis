using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;

namespace Diagnosis.Common.Presentation.Converters
{
    /// <summary>
    /// Показывает текст DateUnit для DateOffset. 
    /// </summary>
    public class DateOffsetToUnitLabel : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d = value as DateOffset;
            if (d == null)
                return DependencyProperty.UnsetValue;

            return DateOffsetFormatter.GetUnitString(d.Offset, d.Unit);
        }
    }

    public class DateOffsetToPartialDateLabel : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d = value as DateOffset;
            if (d == null)
                return DependencyProperty.UnsetValue;

            return DateOffsetFormatter.GetPartialDateString(d);
        }
    }

    /// <summary>
    /// Показывает текст DateUnit для некоторого числа. 
    /// Когда выбираем DateUnit для DateOffset.
    /// </summary>
    public class UnitForOffsetToUnitLabel : BaseMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] is DateUnit) || !(values[1] is int?))
                return DependencyProperty.UnsetValue;


            var unit = (DateUnit)values[0];
            int? offset = (int?)values[1];

            return DateOffsetFormatter.GetUnitString(offset, unit);
        }
    }

    /// <summary>
    /// Показывает текст DateUnit для некоторого числа. 
    /// Когда выбираем DateUnit для DateOffset.
    /// </summary>
    public class HealUnitToUnitLabel : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d = value as DateOffset;
            if (d == null)
                return DependencyProperty.UnsetValue;


            return DateOffsetFormatter.GetPartialDateString(d);
        }
    }
}
