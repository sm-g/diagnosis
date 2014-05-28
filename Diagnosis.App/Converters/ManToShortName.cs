using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Diagnosis.App.Converters
{
    public class ManToShortName : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                dynamic man = value;
                try
                {
                    return man.LastName + (man.FirstName.Length > 0 ? " " + man.FirstName[0] + "." + (man.MiddleName.Length > 0 ? " " + man.MiddleName[0] + "." : "") : "");
                }
                catch
                {
                }
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
