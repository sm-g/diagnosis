using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls.CardParts
{
    public partial class HealthRecordsList : UserControl
    {
        public HealthRecordsList()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
#if !DEBUG
                debug.Visibility = System.Windows.Visibility.Collapsed;
#endif
            };
        }

        private void records_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // fix duplicates in SelectedItems http://stackoverflow.com/questions/27685460/wpf-listbox-extendedmode-move-selected-item-with-isselected-binding
            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    if (records.SelectedItems.Cast<object>().Where(i => i == item).Count() > 1)
                    {
                        records.SelectedItems.Remove(item);
                    }
                }
            }

            records.UpdateLayout();
            records.ScrollIntoView(records.SelectedItem);
        }

        private void item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                // e.Handled = true;
            }
            ;
        }
    }
}