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

        SearchViewModel vm
        {
            get
            {
                return DataContext as SearchViewModel;
            }
        }

        public event EventHandler ResultItemClicked;

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
                var h = ResultItemClicked;
                if (h != null)
                {
                    h(sender, new EventArgs());
                }
            }
            selectedIndex = -1;
            selectionChanged = false;
        }
    }
}