using Diagnosis.App.ViewModels;
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
            //if (e.Key == Key.Up && selectedIndex > 0)
            //{
            //    selectedIndex--;
            //    (results.Items[selectedIndex] as ICheckable).IsSelected = true;
            //}
            //if (e.Key == Key.Down && selectedIndex < results.Items.Count - 1)
            //{
            //    selectedIndex++;
            //    (results.Items[selectedIndex] as ICheckable).IsSelected = true;
            //}
        }

        private void OnResultItemSelected()
        {
            dynamic isearch = DataContext;
            isearch.RaiseResultItemSelected();
        }

    }
}