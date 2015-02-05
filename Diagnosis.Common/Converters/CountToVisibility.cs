using System;
using System.Globalization;

namespace Diagnosis.Common.Converters
{
    public class MoreThanToVisibility : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
    }

    public class LessThanToVisibility : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
    }
}