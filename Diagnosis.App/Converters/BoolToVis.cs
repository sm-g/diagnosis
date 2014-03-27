using System.Windows;

namespace Diagnosis.App.Converters
{
    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed) { }
    }

    public sealed class NegBooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public NegBooleanToVisibilityConverter() :
            base(Visibility.Collapsed, Visibility.Visible) { }
    }
}
