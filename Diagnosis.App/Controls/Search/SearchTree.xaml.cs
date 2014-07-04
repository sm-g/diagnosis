using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls
{
    public partial class SearchTree : UserControl
    {
        private int selectedIndex = -1;

        private object selectedItem;

        public SearchTree()
        {
            InitializeComponent();
        }

        #region Focus stuff

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            EnhancedFocusScope.SetFocusOnActiveElementInScope(this);
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            HidePopup();
        }

        private void input_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FocusChecker.IsFocusOutsideDepObject(this) && FocusChecker.IsFocusOutsideDepObject(popup.Child))
            {
                HidePopup();
            }
        }

        private void input_GotMouseCapture(object sender, MouseEventArgs e)
        {
            ShowPopup();
        }

        private void ShowPopup()
        {
            popup.IsOpen = true;
        }

        private void HidePopup()
        {
            popup.IsOpen = false;
        }

        #endregion Focus stuff

        private void input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && selectedIndex > 0)
            {
                MoveSelection(false);
            }
            else if (e.Key == Key.Down)
            {
                MoveSelection(true);
            }
            else if (e.Key == Key.Enter)
            {
                RaiseSearchSelected();
            }
        }

        private void item_selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = sender as TreeViewItem;
            if (tvi != null)
                tvi.BringIntoView();
            e.Handled = true;
        }

        private void item_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
                RaiseSearchSelected();
        }

        private void item_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var source = e.Source as ContentPresenter;
            selectedItem = source.Content;
            RaiseSearchSelected();
            e.Handled = true;
        }

        private void MoveSelection(bool down)
        {
            int currentSelected = results.SelectedIndex();
            dynamic item; // IHierarchicalCheckable
            do
            {
                selectedIndex += down ? 1 : -1;
                item = results.FindByIndex(selectedIndex);

                // вышли за границы дерева
                if (item == null)
                {
                    selectedIndex = currentSelected;
                    break;
                }
            } while (!(item.IsFiltered && item.IsTerminal)); // только видимые листья

            selectedItem = item;
            if (item != null)
                item.IsSelected = true;
        }

        private void RaiseSearchSelected()
        {
            (DataContext as dynamic).OnSelected(selectedItem);  //  DataContext is PopupSearch<>;
        }
    }
}