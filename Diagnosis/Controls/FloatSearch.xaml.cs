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
        public FloatSearch()
        {
            InitializeComponent();

            TypeDescriptor.GetProperties(this.results)["ItemsSource"].AddValueChanged(this.results, new EventHandler(results_ItemsSourceChanged));
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            EnhancedFocusScope.SetFocusOnActiveElementInScope(floatSearch);
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void results_ItemsSourceChanged(object sender, EventArgs e)
        {
        }

        private void results_SourceUpdated(object sender, DataTransferEventArgs e)
        {
        }

        private void results_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine(results.SelectedIndex + " selected");
        }

        private void input_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}