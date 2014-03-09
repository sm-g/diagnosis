using Diagnosis.Helpers;
using Diagnosis.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.Controls
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

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            EnhancedFocusScope.SetFocusOnActiveElementInScope(floatSearch);
        }

        private void floatSearch_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    OnResultItemSelected();
                    break;

                case Key.Escape:
                    HidePopup();
                    break;
            }
        }

        private void results_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectedIndex == -1)
            {
                selectedIndex = results.SelectedIndex;
            }
            selectionChanged = true;
            results.ScrollIntoView(results.SelectedItem);
        }

        private void input_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowResultsPopup();
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

        private void input_GotFocus(object sender, RoutedEventArgs e)
        {
            ShowResultsPopup();
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
            ShowResultsPopup();
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

        private void ShowResultsPopup()
        {
            popup.IsOpen = true;
        }

        private void HidePopup()
        {
            popup.IsOpen = false;
        }

        private void OnResultItemSelected()
        {
            HidePopup();
            if (DataContext is SearchBase<SymptomViewModel>)
                (DataContext as SearchBase<SymptomViewModel>).RaiseResultItemSelected();
            else if (DataContext is SearchBase<PatientViewModel>)
                (DataContext as SearchBase<PatientViewModel>).RaiseResultItemSelected();
        }
    }
}