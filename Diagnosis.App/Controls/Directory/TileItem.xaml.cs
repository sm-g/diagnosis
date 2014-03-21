
using Diagnosis.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for TileItem.xaml
    /// </summary>
    public partial class TileItem : UserControl
    {
        public TileItem()
        {
            InitializeComponent();
        }


        private void editor_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FocusChecker.IsFocusOutsideDepObject(root))
            {

            }
        }
    }
}