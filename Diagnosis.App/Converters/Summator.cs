using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Diagnosis.App.Converters
{
    public class Summator : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (!(value is double))
                return null;
            var a = (double)value;

            double b = parameter is double ? (double)parameter : 0;

            if (parameter is string)
                double.TryParse(parameter as string, out b);

            if (a + b > 0)
                return a + b;
            return 0; // no negative values
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
