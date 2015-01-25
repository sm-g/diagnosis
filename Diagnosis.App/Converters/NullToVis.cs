using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Diagnosis.App.Converters
{
    public class NullToVisibilityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}