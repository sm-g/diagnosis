using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Diagnosis.Common.Presentation.Converters
{
    public class EqualityToBoolMultiConverter : BaseMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Distinct().Count() == 1;
        }
    }

    /// <summary>
    /// First value is On flag, others comapared.
    /// </summary>
    public class EqualityAndToBoolMultiConverter : BaseMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return ((values[0] as bool?) ?? false) && values.Skip(1).Distinct().Count() == 1;
        }
    }
    public class EqualityToBoolConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }
    }
}
