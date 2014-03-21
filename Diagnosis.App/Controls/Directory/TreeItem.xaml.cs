
using Diagnosis.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for TreeItem.xaml
    /// </summary>
    public partial class TreeItem : UserControl
    {
        public TreeItem()
        {
            InitializeComponent();
        }

        private void editor_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FocusChecker.IsFocusOutsideDepObject(editor))
            {
                
            }
        }

    }
}