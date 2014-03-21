using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Diagnosis.Converters
{
    public class BoolToAligmentConverter<T> : IValueConverter
    {
        public BoolToAligmentConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }

    public sealed class BoolToCenterAligmentConverter : BoolToAligmentConverter<HorizontalAlignment>
    {
        public BoolToCenterAligmentConverter() :
            base(HorizontalAlignment.Center, HorizontalAlignment.Left) { }
    }
}
