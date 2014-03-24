using Diagnosis.App.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls
{
    /// <summary>
    /// Interaction logic for FloatSearch.xaml
    /// </summary>
    public partial class FloatSearch : UserControl
    {
        private int selectedIndex = -1;
        private bool selectionChanged;

        public FloatSearch()
        {
            InitializeComponent();
        }

        #region Focus stuff

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            EnhancedFocusScope.SetFocusOnActiveElementInScope(floatSearch);
        }

        private void input_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowPopup();
        }

        private void input_GotFocus(object sender, RoutedEventArgs e)
        {
            ShowPopup();
        }

        private void input_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FocusChecker.IsFocusOutsideDepObject(floatSearch) && FocusChecker.IsFocusOutsideDepObject(popup.Child))
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
            HidePopup();
            var context = DataContext as ISearchCommon;
            if (context != null)
            {
                context.RaiseResultItemSelected();
            }
        }
    }
}