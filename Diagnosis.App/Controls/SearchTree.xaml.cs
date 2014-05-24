using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Diagnosis.App.Controls
{
    public partial class SearchTree : UserControl
    {
        private int selectedIndex = -1;
        private bool selectionChanged;

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
                //dynamic isearch = DataContext;
                //isearch.SelectedIndex = isearch.searcher.Collection.IndexOf(selectedItem);
                //isearch.RaiseResultItemSelected();
            }
        }
        object selectedItem;
        private void MoveSelection(bool down)
        {
            int currentSelected = SelectedIndex(results);
            dynamic item;
            do
            {
                selectedIndex += down ? 1 : -1;
                item = FindInTree(results, selectedIndex);

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

        private object FindInTree(ItemsControl parentContainer, int searchIndex)
        {
            int index = 0;
            return FindInTree(parentContainer, ref index, searchIndex);
        }

        private object FindInTree(ItemsControl parentContainer, ref int index, int searchIndex)
        {
            foreach (var item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (currentContainer != null)
                {
                    if (index == searchIndex)
                    {
                        return item;
                    }

                    index++;

                    var inner = FindInTree(currentContainer, ref index, searchIndex);
                    if (inner != null)
                    {
                        return inner;
                    }
                }
            }
            return null;
        }

        private int FindSelectedIndex(ItemCollection items, int index)
        {
            foreach (var _item in items)
            {
                if (_item == results.SelectedItem)
                {
                    return index;
                }
                index++;
                var inner = FindSelectedIndex(_item as ItemCollection, index);
                if (inner > -1)
                {
                    return inner;
                }
            }

            return -1;
        }

        private static bool FindSelectedIndex(ItemsControl parentContainer, ref int index)
        {
            foreach (var item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (currentContainer != null)
                {
                    if (currentContainer.IsSelected && currentContainer.IsVisible)
                        return true;
                    index++;

                    if (currentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    {
                        // If the sub containers of current item is ready, we can directly go to the next
                        // iteration to expand them.

                        if (FindSelectedIndex(currentContainer, ref index))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        int SelectedIndex(ItemsControl parentContainer)
        {
            int selectedIndex = 0;
            if (FindSelectedIndex(results, ref selectedIndex))
            {
                return selectedIndex;
            }
            else
            {
                return -1;
            }
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