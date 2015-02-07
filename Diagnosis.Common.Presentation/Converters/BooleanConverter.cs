using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Diagnosis.Common.Presentation.Converters
{
    public class BooleanConverter<T> : BaseValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }

    public sealed class NegateConverter : BooleanConverter<Boolean>
    {
        public NegateConverter() :
            base(false, true) { }
    }

    public class IsNullToBooleanConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }
    }

    public class NullableBooleanConverter<T> : BaseValueConverter
    {
        public NullableBooleanConverter(T trueValue, T falseValue, T nullValue)
        {
            True = trueValue;
            False = falseValue;
            Null = nullValue;
        }

        public T True { get; set; }
        public T False { get; set; }
        public T Null { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = value as bool?;
            if (b == null)
                return Null;
            return b.Value ? True : False;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is T)
            {
                if (EqualityComparer<T>.Default.Equals((T)value, True))
                    return true;
                if (EqualityComparer<T>.Default.Equals((T)value, False))
                    return false;
            }
            return null;
        }
    }

    /// <summary>
    /// For Radio button
    /// </summary>
    public class NullableBooleanConverter : BaseValueConverter
    {
        // http://stackoverflow.com/a/18451465/3009578
        // works if initial is null. if initial is false, cannot set to true (double OnPropertyChanged?)
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var test = (bool?)value;
            var result = bool.Parse((string)parameter);
            if (test == result)
            {
                return true;
            }

            return false;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = bool.Parse((string)parameter);
            return result;
        }
    }

    public class AndMultiConverter : BaseMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.All(x => (x as bool?) ?? false);
        }
    }

    public class OrMultiConverter : BaseMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Any(x => (x as bool?) ?? false);
        }
    }
}
