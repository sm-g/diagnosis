using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Collections;

namespace Diagnosis.App.Controls.Search
{
    public partial class SearchTree : UserControl
    {
        private int selectedIndex = -1;

        private dynamic selectedItem; // IHierarchicalCheckable

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
            if (e.Key == Key.Up)
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
            if (source != null)
            {
                dynamic item = source.Content;
                if (item.IsTerminal)
                {
                    // выбираем только дистья
                    RaiseSearchSelected();
                }
                e.Handled = true;
            }
        }

        private void MoveSelection(bool down)
        {
            int currentSelected = results.SelectedIndex();
            dynamic item;
            if (currentSelected == -1 && !down)
            {
                // ничего не выбрано, нажимаем вверх — выбираем последний элемент из видимых
                var lastInd = -1;
                do
                {
                    selectedIndex++;
                    item = results.FindByIndex(selectedIndex);

                    if (item == null) // пока не дошли до конца
                        break;

                    if (item.IsExpanded && item.IsTerminal)
                        lastInd = selectedIndex;
                } while (true);

                selectedIndex = lastInd;
            }
            else
            {
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
                } while (!(item.IsExpanded && item.IsTerminal)); // пока не найдём видимый лист
            }

            item = results.FindByIndex(selectedIndex);
            if (item != null && item.IsExpanded && item.IsTerminal)
            {
                item.IsSelected = true;
            }

            selectedItem = item;
        }

        private void RaiseSearchSelected()
        {
            if (selectedItem == null)
            {
                MoveSelection(true); // выбираем первый элемент
            }
            if (selectedItem != null)
                (DataContext as dynamic).SelectReal(selectedItem);  //  DataContext is PopupSearch<>;
        }
    }
}