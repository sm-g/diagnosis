using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Diagnosis.Models;

namespace Diagnosis.App.Converters
{
    public class HrToItemsString : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var hr = value as HealthRecord;
            if (hr != null)
            {
                return string.Join(" ", hr.GetOrderedEntities());
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
