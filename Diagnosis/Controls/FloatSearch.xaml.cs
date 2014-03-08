using Diagnosis.Helpers;
using Diagnosis.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for FloatSearch.xaml
    /// </summary>
    public partial class FloatSearch : UserControl
    {
        int selectedIndex = -1;
        bool selectionChanged;

        public FloatSearch()
        {
            InitializeComponent();
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            EnhancedFocusScope.SetFocusOnActiveElementInScope(floatSearch);
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
                RaiseResultItemSelected();
            }
            selectedIndex = -1;
            selectionChanged = false;
        }

        private void floatSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RaiseResultItemSelected();
            }
        }

        private void RaiseResultItemSelected()
        {
            if (DataContext is SearchViewModel<SymptomViewModel>)
                (DataContext as SearchViewModel<SymptomViewModel>).RaiseResultItemSelected();
            else if (DataContext is SearchViewModel<PatientViewModel>)
                (DataContext as SearchViewModel<PatientViewModel>).RaiseResultItemSelected();
        }
    }
}