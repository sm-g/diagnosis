using Diagnosis.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.App.Controls
{
    /// <summary>
    /// Interaction logic for InputTextBox.xaml
    /// </summary>
    public partial class InputTextBox : UserControl
    {
        public InputTextBox()
        {
            InitializeComponent();
        }

        private AutoCompleteBoxViewModel vm
        {
            get
            {
                return this.DataContext as AutoCompleteBoxViewModel;
            }
        }

        private void auto_TextChanged(object sender, RoutedEventArgs e)
        {
            Debug.Print("autocompletebox text = {0}", auto.Text);
            if (auto.Text.Length == auto.CaretIndex + 1 && auto.Text.Last() == vm.DelimSpacer)
            {
                auto.CaretIndex = auto.Text.Length;
            }
        }
    }
}