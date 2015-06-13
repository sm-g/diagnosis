using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Diagnosis.Common;
using System.Windows;

namespace Diagnosis.Common.Presentation.Converters
{
    public class AgeToYearsLabel : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is int))
                return DependencyProperty.UnsetValue;

            int age = (int)value;
            var index = PluralsHelper.GetPluralEnding(age);

            return PluralsHelper.years[index];
        }
    }
}
