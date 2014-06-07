using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Diagnosis.Core;

namespace Diagnosis.App.Converters
{
    public class DateIntervalLabel : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (values.Length < 1)
                return null;

            var fromLabel = "с";
            var toLabel = "по";
            if (parameter != null)
            {
                var labels = parameter as IList<string>;
                if (labels != null && labels.Count > 0)
                {
                    fromLabel = labels.First();
                    toLabel = labels.Last();
                }
            }

            DateTime? from = (DateTime?)(values[0] is DateTime? ? values[0] : null);
            DateTime? to = (DateTime?)(values.Length > 1 && values[1] is DateTime? ? values[1] : null);

            if (!(from.HasValue || to.HasValue))
                return null;

            Tuple<string, string> formats;

            if (from.HasValue)
            {
                formats = DateFormatter.GetFormat(from.Value, to);
            }
            else // only to.HasValue
            {
                formats = DateFormatter.GetFormat(to.Value, null);
            }

            if (from.HasValue && to.HasValue)
                return string.Format("{0} {1} {2} {3}", fromLabel, from.Value.ToString(formats.Item1), toLabel, to.Value.ToString(formats.Item2));
            else
            {
                if (from.HasValue)
                    return string.Format("{0} {1}", fromLabel, from.Value.ToString(formats.Item1));
                else // to.HasValue
                    return string.Format("{0} {1}", toLabel, to.Value.ToString(formats.Item1));
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class DateLabel : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            DateTime from = (DateTime)value;

            var formats = DateFormatter.GetFormat(from, null);

            return string.Format("{0} {1}", parameter ?? "", from.ToString(formats.Item1));
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
