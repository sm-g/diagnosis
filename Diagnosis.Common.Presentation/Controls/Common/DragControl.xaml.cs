using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.Common.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for Drag.xaml
    /// </summary>
    public partial class DragControl : UserControl
    {
        public DragControl()
        {
            InitializeComponent();
        }

        public bool IsCircle
        {
            get { return (bool)GetValue(IsCircleProperty); }
            set { SetValue(IsCircleProperty, value); }
        }

        public static readonly DependencyProperty IsCircleProperty =
            DependencyProperty.Register("IsCircle", typeof(bool), typeof(DragControl), new PropertyMetadata(false));
    }
}