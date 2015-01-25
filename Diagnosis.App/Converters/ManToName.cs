using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Diagnosis.Common;
using Diagnosis.Models;

namespace Diagnosis.App.Converters
{
    public class ManToName : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IMan)
            {
                bool full = false;
                bool.TryParse(parameter as string, out full);
                if (full)
                {
                    var name = NameFormatter.GetFullName(value as IMan);
                    return name ?? ((value is Patient) ? (value as Patient).CreatedAt.ToString("dd.MM.yy hh:mm") : "");
                }
                else
                    return NameFormatter.GetShortName(value as IMan);
            }
            return "";
        }
    }
}
