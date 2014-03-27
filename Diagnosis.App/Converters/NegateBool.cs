using System;

namespace Diagnosis.App.Converters
{
    public sealed class NegateConverter : BooleanConverter<Boolean>
    {
        public NegateConverter() :
            base(false, true) { }
    }
}