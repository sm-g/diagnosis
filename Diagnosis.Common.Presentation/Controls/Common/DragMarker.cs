using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Diagnosis.Common.Presentation.Controls
{
    public class DragMarker : FrameworkElement
    {
        public DragMarker()
        {
            this.SnapsToDevicePixels = true;
        }

        public static readonly DependencyProperty ForegroundProperty =
            Control.ForegroundProperty.AddOwner(typeof(DragMarker));

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public bool IsCircle
        {
            get { return (bool)GetValue(IsCircleProperty); }
            set { SetValue(IsCircleProperty, value); }
        }

        public static readonly DependencyProperty IsCircleProperty =
            DependencyProperty.Register("IsCircle", typeof(bool), typeof(DragMarker), new PropertyMetadata(false));

        protected override void OnRender(DrawingContext drawingContext)
        {
            for (int y = 0; y < (int)ActualHeight / 4; y++)
            {
                for (int x = 0; x < (int)ActualWidth / 4; x++)
                {
                    if (IsCircle)
                    {
                        drawingContext.DrawEllipse(Foreground, null,
                            new Point(x * 4 + 2, y * 4 + 2), 1, 1);
                    }
                    else
                    {
                        drawingContext.DrawRectangle(Foreground, null,
                            new Rect(x * 4, y * 4, 2, 2));
                    }
                }
            }
        }
    }
}