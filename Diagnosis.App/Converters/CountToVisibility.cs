using System;
using System.Globalization;
using System.Windows.Data;

namespace Diagnosis.App.Converters
{
    public class MoreThanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            int count = (int)value;
            int limit = 0;

            if (parameter != null)
                int.TryParse((string)parameter, out limit);

            if (count > limit)
            {
                return System.Windows.Visibility.Visible;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class LessThanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            int count = (int)value;
            int limit = 1;

            if (parameter != null)
                int.TryParse((string)parameter, out limit);

            if (count < limit)
            {
                return System.Windows.Visibility.Visible;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
