using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Diagnosis.ViewModels;
using Diagnosis.Common;

namespace Diagnosis.App.Converters
{
    public class CountToAppointmentsLabel : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            int count = (int)value;

            var index = Plurals.GetPluralEnding(count);

            return new[] { "осмотр", "осмотра", "осмотров" }[index];
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
