using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Diagnosis.Helpers;
using Diagnosis.ViewModels;

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

        void results_ItemsSourceChanged(object sender, EventArgs e)
        {
           // results.SelectedIndex = 0;
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
            if (e.Key == Key.Enter)
            {
                (DataContext as SearchViewModel).SelectedItem.IsChecked = true;
            }
        }

    }
}
