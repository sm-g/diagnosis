using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Diagnosis.App.Converters
{
    public class IsLastItem : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return false;
            var list = values[1] as IList;
            if (list == null)
                return false;
            return object.Equals(values[0], list[list.Count - 1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
