using Diagnosis.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Diagnosis.App.Converters
{
    public class IHierarchicalCheckableToBackground : IValueConverter
    {
        private static Brush noCheckedChildBrush = new LinearGradientBrush(Colors.Beige, Colors.Transparent, 0);
        private static Brush isCheckedChildBrush = new LinearGradientBrush(Colors.Turquoise, Colors.Transparent, 0);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // trap for DisconnectedITem
            if (!(value is ICheckable))
            {
                return null;
            }

            dynamic s = value; // IHierarchicalCheckable
            try
            {
                if (s.CheckedChildren == 0)
                    return noCheckedChildBrush;
                else
                    return isCheckedChildBrush;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}