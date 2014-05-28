﻿using Diagnosis.App.ViewModels;
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
            var s = value as IHierarchicalCheckable;
            if (s != null)
            {
                if (s.CheckedChildren == 0)
                    return noCheckedChildBrush;
                else
                    return isCheckedChildBrush;
            }
            else
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