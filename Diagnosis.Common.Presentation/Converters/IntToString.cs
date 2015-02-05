using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace Diagnosis.Common.Presentation.Converters
{
    public class IntToString : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is int)
            {
                return ((int)value).ToString();
            }
            return DependencyProperty.UnsetValue;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (string)value == "")
            {
                return null;
            }

            return int.Parse(value as string);
        }
    }
}
