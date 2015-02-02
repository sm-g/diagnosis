using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Diagnosis.Models;
using Diagnosis.Common.Converters;

namespace Diagnosis.App.Converters
{
    public class HrToItemsString : BaseValueConverter
    {
        public override object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var hr = value as HealthRecord;
            if (hr != null)
            {
                return string.Join(" ", hr.GetOrderedEntities());
            }
            return "";
        }
    }
}
