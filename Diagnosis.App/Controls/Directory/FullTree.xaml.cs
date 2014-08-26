using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.App.Controls
{
    /// <summary>
    /// Interaction logic for TreeView.xaml
    /// </summary>
    public partial class FullTree : UserControl
    {
        public FullTree()
        {
            InitializeComponent();
        }

        private void item_selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = sender as TreeViewItem;
            if (tvi != null)
            {
                tvi.BringIntoView();
                tvi.Focus();
            }
            e.Handled = true;
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(FullTree));
    }
}