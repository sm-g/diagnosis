using System.Windows;

namespace Diagnosis.Common.Presentation.Behaviors
{
    public class Square
    {
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.RegisterAttached("Size", typeof(double?), typeof(Square), new PropertyMetadata(null, (sender, e) =>
              {
                  if (((double?)e.NewValue).HasValue)
                  {
                      var element = sender as FrameworkElement;
                      if (element == null) return;

                      var size = ((double?)e.NewValue).Value;
                      element.Width = size;
                      element.Height = size;
                  }
              }));

        public static double? GetSize(DependencyObject obj)
        {
            return (double?)obj.GetValue(SizeProperty);
        }

        public static void SetSize(DependencyObject obj, double? value)
        {
            obj.SetValue(SizeProperty, value);
        }
    }
}