using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Diagnosis.App.Converters
{
    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
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

    public sealed class NegateConverter : BooleanConverter<Boolean>
    {
        public NegateConverter() :
            base(false, true) { }
    }

    public class NullableBooleanConverter<T> : IValueConverter
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

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = value as bool?;
            if (b == null)
                return Null;
            return b.Value ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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

    public class NullableBooleanConverter : IValueConverter
    {
        // http://stackoverflow.com/a/18451465/3009578
        // works if initial is null. if initial is false, cannot set to true (double OnPropertyChanged?)
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var test = (bool?)value;
            var result = bool.Parse((string)parameter);
            if (test == result)
            {
                return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = bool.Parse((string)parameter);
            return result;
        }
    }
}
