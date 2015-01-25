using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Diagnosis.App.Converters
{
    public class InListConverter : BaseMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return false;
            var col = values[1] as IList;
            if (col == null)
                return false;

            return col.Contains(values[0]);
        }
    }
    public class IsLastItem : BaseMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return false;
            var list = values[1] as IList;
            if (list == null)
                return false;
            if (list.Count <= 1)
                return true;
            return object.Equals(values[0], list[list.Count - 1]);
        }
    }
}
