using System.Windows;
using System.Windows.Controls;
using Diagnosis.Common.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls
{
    /// <summary>
    /// Interaction logic for RadioListBox.xaml
    /// </summary>
    public partial class RadioListBox : ListBox
    {
        public RadioListBox()
        {
            InitializeComponent();
        }

        public bool MultiSelection
        {
            get { return (bool)GetValue(MultiSelectionProperty); }
            set { SetValue(MultiSelectionProperty, value); }
        }

        public static readonly DependencyProperty MultiSelectionProperty =
            DependencyProperty.Register("MultiSelection", typeof(bool), typeof(RadioListBox));

        /// <summary>
        /// Changes IsSelected for parent ListBoxItem.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var chkBox = sender as CheckBox;
            if (chkBox != null)
            {
                var parent = chkBox.FindAncestorOrSelf<ListBoxItem>();
                parent.IsSelected = chkBox.IsChecked.Value;
            }
        }

        private void ListBoxItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var item = sender as ListBoxItem;
                if (item != null)
                {
                    var chkBox = item.FindChild<CheckBox>("chkbox");
                    if (chkBox != null)
                        chkBox.IsChecked = !chkBox.IsChecked;
                }
            }

        }
    }
}