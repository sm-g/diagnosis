// http://www.codeproject.com/Articles/29495/Binding-and-Using-Friendly-Enums-in-WPF

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace Diagnosis.Common.Presentation.Converters
{
    /// <summary>
    /// This class simply takes an enum and uses some reflection to obtain
    /// the friendly name for the enum. Where the friendlier name is
    /// obtained using the LocalizableDescriptionAttribute, which holds the localized
    /// value read from the resource file for the enum
    /// </summary>
    [ValueConversion(typeof(object), typeof(String))]
    public class EnumToLabel : BaseValueConverter
    {
        /// <summary>
        /// Convert value for binding from source object
        /// </summary>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // To get around the stupid WPF designer bug
            if (value != null)
            {
                return Diagnosis.Common.AttributesHelper.GetEnumDescription(value);
            }

            return string.Empty;
        }
    }
}