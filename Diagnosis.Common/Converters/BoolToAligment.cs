using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Diagnosis.Common.Converters
{
    public sealed class BoolToCenterAligmentConverter : BooleanConverter<HorizontalAlignment>
    {
        public BoolToCenterAligmentConverter() :
            base(HorizontalAlignment.Center, HorizontalAlignment.Left) { }
    }
}
