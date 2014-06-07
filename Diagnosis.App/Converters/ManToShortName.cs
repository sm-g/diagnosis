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
                    string ln = man.LastName ?? "";
                    string mn = man.MiddleName ?? "";
                    string fn = man.FirstName ?? "";

                    // короткое имя человека в форматах:
                    // Иванов И. И.
                    // Иванов И.
                    // Иванов
                    // Иван И.
                    // Иван
                    if (ln.Length > 0)
                        return ln + (fn.Length > 0 ? " " + fn[0] + "." + (mn.Length > 0 ? " " + mn[0] + "." : "") : "");
                    else
                        if (fn.Length > 0)
                            return fn + (mn.Length > 0 ? " " + mn[0] + "." : "");
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
