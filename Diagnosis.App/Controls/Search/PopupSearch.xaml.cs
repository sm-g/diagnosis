using Diagnosis.App.Behaviors;
using Diagnosis.Common.Behaviors;
using Diagnosis.Common.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls.Search
{
    /// <summary>
    /// Interaction logic for PopupSearch.xaml
    /// </summary>
    public partial class PopupSearch : UserControl
    {
        private int selectedIndex = -1;
        private bool selectionChanged;

        public PopupSearch()
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
            if (FocusChecker.IsLogicFocusOutside(this) && FocusChecker.IsLogicFocusOutside(popup.Child))
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

        private void results_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectedIndex == -1)
            {
                selectedIndex = results.SelectedIndex;
            }
            selectionChanged = true;
            results.ScrollIntoView(results.SelectedItem);
        }

        private void input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && results.SelectedIndex > 0)
            {
                results.SelectedIndex--;
            }
            if (e.Key == Key.Down && results.SelectedIndex < results.Items.Count - 1)
            {
                results.SelectedIndex++;
            }
        }

        private void results_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!selectionChanged || selectedIndex == results.SelectedIndex)
            {
                OnResultItemSelected();
            }
            selectedIndex = -1;
            selectionChanged = false;
        }

        private void OnResultItemSelected()
        {
            dynamic isearch = DataContext; // PopupSearch
            isearch.RaiseResultItemSelected();
        }
    }
}