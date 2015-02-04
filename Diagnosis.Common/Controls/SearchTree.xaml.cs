using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.Common.Controls.Search
{
    public partial class SearchTree : UserControl
    {
        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.Register("Collection", typeof(IEnumerable), typeof(SearchTree));



        public Panel AfterFilterPanel
        {
            get { return (Panel)GetValue(AfterFilterPanelProperty); }
            set { SetValue(AfterFilterPanelProperty, value); }
        }

        public static readonly DependencyProperty AfterFilterPanelProperty =
            DependencyProperty.Register("AfterFilterPanel", typeof(Panel), typeof(SearchTree), new PropertyMetadata(null));



        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(SearchTree));
        private int selectedIndex = -1;
        private dynamic selectedItem; // IHierarchicalCheckable

        public SearchTree()
        {
            InitializeComponent();
            results1.SelectedItemChanged += (s, e) =>
            {
                if (e.NewValue != null)
                {
                    RaiseSearchSelected(e.NewValue);
                }
            };
        }

        public IEnumerable Collection
        {
            get { return (IEnumerable)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }
        #region Focus stuff

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            //  EnhancedFocusScope.SetFocusOnActiveElementInScope(this);
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FocusChecker.IsLogicFocusOutside(this)
                // && FocusChecker.IsFocusOutsideDepObject(popup.Child)
               )
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
            // popup.IsOpen = true;
        }

        private void HidePopup()
        {
            //  popup.IsOpen = false;
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
                RaiseSearchSelected(selectedItem);
            }
        }

        private void item_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                var source = e.Source as TreeViewItem;
                if (source != null)
                {
                    dynamic item = source.Header;
                    RaiseSearchSelected(item);
                }
            }
        }

        private void item_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ContentPresenter content = e.Source as ContentPresenter;

            // click on border or on content itself
            //var bd = e.Source as Border;
            //if (bd != null)
            //{
            //    content = bd.FindName("PART_Header") as ContentPresenter;
            //}

            //if (content != null)
            //{
            //    dynamic item = content.Content;
            //    RaiseSearchSelected(item);
            //    e.Handled = true;
            //}
        }

        private void MoveSelection(bool down)
        {
            var tree = results1; // results.TreeView;
            int currentSelected = tree.SelectedIndex();
            logger.DebugFormat("search tree current: {0}", currentSelected);

            dynamic item;
            if (currentSelected == -1 && !down)
            {
                // ничего не выбрано, нажимаем вверх — выбираем последний элемент из видимых
                var lastInd = -1;
                do
                {
                    selectedIndex++;
                    item = tree.FindByIndex(selectedIndex);

                    if (item == null) // пока не дошли до конца
                        break;

                    if (item.IsTerminal)
                        lastInd = selectedIndex;
                } while (true);

                selectedIndex = lastInd;
            }
            else
            {
                do
                {
                    selectedIndex += down ? 1 : -1;
                    item = tree.FindByIndex(selectedIndex);
                    logger.DebugFormat("select search tree {0}: {1}", selectedIndex, item);

                    // вышли за границы дерева
                    if (item == null)
                    {
                        selectedIndex = currentSelected;
                        break;
                    }
                } while (!(item.IsTerminal)); // пока не найдём видимый лист
            }

            item = tree.FindByIndex(selectedIndex);
            if (item != null && item.IsTerminal)
            {
                // logger.DebugFormat("bef sel");

                item.IsSelected = true;

                // logger.DebugFormat("aft sel");
            }

            selectedItem = item;
            input.Focus();
        }

        private void RaiseSearchSelected(dynamic item)
        {
            if (item == null)
            {
                MoveSelection(true); // выбираем первый элемент
            }
            // выбираем только листья
            if (item != null && item.IsTerminal)
                (DataContext as dynamic).RaiseResultItemSelected(item);  //  DataContext is PopupSearch<>;
        }
    }
}