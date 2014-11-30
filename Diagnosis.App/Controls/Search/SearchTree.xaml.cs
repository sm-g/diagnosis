using Diagnosis.ViewModels.Search;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls.Search
{
    public partial class SearchTree : UserControl
    {
        private int selectedIndex = -1;
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(SearchTree));
        private dynamic selectedItem; // IHierarchicalCheckable

        public IEnumerable Collection
        {
            get { return (IEnumerable)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }

        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.Register("Collection", typeof(IEnumerable), typeof(SearchTree));

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
                    // выбираем только листья
                    RaiseSearchSelected();
                }
                e.Handled = true;
            }
        }

        private void MoveSelection(bool down)
        {
            int currentSelected = results.TreeView.SelectedIndex();
            dynamic item;
            if (currentSelected == -1 && !down)
            {
                // ничего не выбрано, нажимаем вверх — выбираем последний элемент из видимых
                var lastInd = -1;
                do
                {
                    selectedIndex++;
                    item = results.TreeView.FindByIndex(selectedIndex);

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
                    item = results.TreeView.FindByIndex(selectedIndex);
                    logger.DebugFormat("select search tree {0}: {1}", selectedIndex, item);

                    // вышли за границы дерева
                    if (item == null)
                    {
                        selectedIndex = currentSelected;
                        break;
                    }
                } while (!(item.IsExpanded && item.IsTerminal)); // пока не найдём видимый лист
            }

            item = results.TreeView.FindByIndex(selectedIndex);
            if (item != null && item.IsExpanded && item.IsTerminal)
            {
                logger.DebugFormat("bef sel");

                item.IsSelected = true;

                logger.DebugFormat("aft sel");

            }
            else
            {
                ;
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
                (DataContext as dynamic).RaiseResultItemSelected(selectedItem);  //  DataContext is PopupSearch<>;
        }
    }
}