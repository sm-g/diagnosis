using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Diagnosis.App.Converters
{
    public class AgeToYearsLabel : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            int age = (int)value;
            if (age % 10 == 0 || age % 10 >= 5 || (age >= 11 && age <= 14))
            {
                return "лет";
            }
            if (age % 10 == 1)
            {
                return "год";
            }
            return "года";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
