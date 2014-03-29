
using Diagnosis.App.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls
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

        private void tileItem_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FocusChecker.IsFocusOutsideDepObject(tileItem))
                search.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}