using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls.CardParts
{
    public partial class HealthRecordsList : UserControl
    {
        public HealthRecordsList()
        {
            InitializeComponent();
        }

        private void records_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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