using Diagnosis.App.Controls;
using Diagnosis.Common.Converters;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Diagnosis.Common.Controls;
using System.Windows.Data;


namespace Diagnosis.App.Converters
{
    public class TreeDepthToMargin : BaseValueConverter
    {
        public double Length { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (item == null)
                return new Thickness(0);

            return new Thickness(Length * item.GetDepth(), 0, 0, 0);
        }
    }
}
