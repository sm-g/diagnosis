using System.Linq;
using System.Windows;

namespace Diagnosis.App.Converters
{
    /// <summary>
    /// IsMale? to string from 2-space separated string. 
    /// Not null parameter forces showing SexUnknown string.
    /// </summary>
    public sealed class BoolToSexSign : BaseMultiValueConverter
    {
        public override object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] is bool?))
                return DependencyProperty.UnsetValue;

            var signs = values[1] as string;
            if (signs == null || signs.Count(x => x == ' ') != 2)
                signs = "♂ ♀ ?";

            var s = signs.Split(' ');
            var b = values[0] as bool?;
            if (b == null)
            {
                if (parameter != null)
                    return s[2];
                else
                    return "";
            }
            return b.Value ? s[0] : s[1];
        }
    }
}