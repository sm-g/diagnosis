using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Diagnosis.Helpers;

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for TreeView.xaml
    /// </summary>
    public partial class FullTree : UserControl
    {
        TreeItem currentItem;

        public FullTree()
        {
            InitializeComponent();
        }

        private void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
            tree.DataContext = Diagnosis.ViewModels.SymptomViewModel.CreateSymptoms();
        }

        TreeItem FindTreeItem(object sender)
        {
            return ChildFinder.FindChild<TreeItem>((DependencyObject)sender, "treeItem");
        }

        private void item_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void item_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var q = FindTreeItem(sender);

            //q.CommitChanges();
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
                currentItem = q;
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
