using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Windows;

using Diagnosis.ViewModels;
using System.Windows.Media;

namespace Diagnosis.Converters
{
    public class SymptomToBackground : IValueConverter
    {
        static Brush noCheckedChildBrush = new LinearGradientBrush(Colors.Beige, Colors.Transparent, 0);
        static Brush isCheckedChildBrush = new LinearGradientBrush(Colors.Turquoise, Colors.Transparent, 0);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try // because value is DisconnectedItem when switching back to tabItem with DataContext=PatientViewModel 
            {
                var s = value as SymptomViewModel;
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
            throw new System.NotImplementedException();
        }
    }
}
