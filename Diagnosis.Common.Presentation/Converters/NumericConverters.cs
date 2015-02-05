using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Diagnosis.Common.Presentation.Converters
{
    public class LessThanConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double) || parameter == null)
                return DependencyProperty.UnsetValue;

            var a = (double)value;

            double b = parameter is double ? (double)parameter : 0;

            if (parameter is string)
                double.TryParse(parameter as string, out b);

            return a < b;
        }
    }

    public class SummatorConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (!(value is double))
                return DependencyProperty.UnsetValue;

            var a = (double)value;

            double b = parameter is double ? (double)parameter : 0;

            if (parameter is string)
                double.TryParse(parameter as string, out b);

            if (a + b > 0)
                return a + b;
            return 0; // no negative values
        }
    }
}