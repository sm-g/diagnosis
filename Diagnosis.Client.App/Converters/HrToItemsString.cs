using Diagnosis.Common.Presentation.Converters;
using Diagnosis.Models;
using System;
using System.Globalization;
using System.Linq;

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