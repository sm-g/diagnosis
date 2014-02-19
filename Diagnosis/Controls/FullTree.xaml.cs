using Diagnosis.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.Controls
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

        private void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
            tree.DataContext = Diagnosis.DataCreator.CreateSymptoms();
        }

        private TreeItem FindTreeItem(object sender)
        {
            return ChildFinder.FindChild<TreeItem>((DependencyObject)sender, "treeItem");
        }

        private void item_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            var q = FindTreeItem(sender);
            if (e.Key == Key.Space)
            {
                q.ToggleCheckedState();
            }
            if (e.Key == Key.F2)
            {
                q.BeginEdit();
            }
            if (e.Key == Key.Enter)
            {
                q.CommitChanges();
            }
            if (e.Key == Key.Escape)
            {
                q.RevertChanges();
            }
            if (e.Key == Key.Insert || e.Key == Key.F && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                q.BeginSearch();
            }
        }

        private void item_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var q = FindTreeItem(sender);
            q.ToggleCheckedState();
            (sender as TreeViewItem).IsExpanded = true;
        }

        private void item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var q = FindTreeItem(sender);

            if ((sender as TreeViewItem).IsSelected) // to prevent multiplay raising http://stackoverflow.com/questions/2280049
                q.BeginEdit();
        }
    }
}