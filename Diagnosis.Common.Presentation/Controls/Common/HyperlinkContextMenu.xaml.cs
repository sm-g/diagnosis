using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Diagnosis.Common.Presentation.Controls
{
    public partial class HyperlinkContextMenu : UserControl
    {
        public HyperlinkContextMenu()
        {
            InitializeComponent();
        }

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(HyperlinkContextMenu),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public IEnumerable MenuItemsSource
        {
            get { return (IEnumerable)GetValue(MenuItemsSourseProperty); }
            set { SetValue(MenuItemsSourseProperty, value); }
        }

        public static readonly DependencyProperty MenuItemsSourseProperty =
            DependencyProperty.Register("MenuItemsSource", typeof(IEnumerable), typeof(HyperlinkContextMenu),
            new PropertyMetadata(null, (s, e) =>
            {
                var hcm = s as HyperlinkContextMenu;
                if (hcm != null)
                {
                    BindingOperations.SetBinding(hcm.menu, ContextMenu.ItemsSourceProperty, new Binding("MenuItemsSource") { Source = hcm });
                }
            }));

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            menu.IsOpen = true;
        }
    }
}