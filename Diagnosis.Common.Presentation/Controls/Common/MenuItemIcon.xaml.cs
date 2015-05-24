using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Diagnosis.Common.Presentation.Controls
{
    public partial class MenuItemIcon : UserControl
    {
        public static readonly DependencyProperty SourceProperty =
           DependencyProperty.Register("Source", typeof(Visual), typeof(MenuItemIcon));

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(MenuItemIcon));

        public MenuItemIcon()
        {
            InitializeComponent();
        }
        public Visual Source
        {
            get { return (Visual)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }
    }
}