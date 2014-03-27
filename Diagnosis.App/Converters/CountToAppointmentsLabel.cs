using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Diagnosis.App.ViewModels;

namespace Diagnosis.App.Converters
{
    public class CountToAppointmentsLabel : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            int count = (int)value;
            if (count % 10 == 0 || count % 10 >= 5 || (count >= 11 && count <= 14))
            {
                return "осмотров";
            }
            if (count % 10 == 1)
            {
                return "осмотр";
            }
            return "осмотра";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
