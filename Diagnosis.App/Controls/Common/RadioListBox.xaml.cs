using System.Windows;
using System.Windows.Controls;

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
                var parent = ParentFinder.FindAncestorOrSelf<ListBoxItem>(chkBox);
                parent.IsSelected = chkBox.IsChecked.Value;
            }
        }
    }
}