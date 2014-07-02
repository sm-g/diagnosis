using Diagnosis.App;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls
{
    /// <summary>
    /// Interaction logic for GroupsTree.xaml
    /// </summary>
    public partial class GroupsTree : TreeCommon
    {
        public GroupsTree()
        {
            InitializeComponent();
        }
        private void item_selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = sender as TreeViewItem;
            if (tvi != null)
                tvi.BringIntoView();
            e.Handled = true;
        }
    }
}
