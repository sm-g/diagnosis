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
using Diagnosis.App.ViewModels;
using System.Windows.Shapes;

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
            Console.WriteLine("auto text = {0}", auto.Text);
            if (auto.Text.Length == auto.CaretIndex + 1 && auto.Text.Last() == vm.DelimSpacer)
            {
                auto.CaretIndex = auto.Text.Length;
            }
        }
    }
}
