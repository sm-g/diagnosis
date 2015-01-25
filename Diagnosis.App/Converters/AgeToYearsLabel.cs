using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Diagnosis.Common;

namespace Diagnosis.App.Converters
{
    public class AgeToYearsLabel : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            int age = (int)value;

            var index = Plurals.GetPluralEnding(age);

            return Plurals.years[index];
        }
    }
}
