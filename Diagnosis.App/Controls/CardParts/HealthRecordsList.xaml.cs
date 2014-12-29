using Diagnosis.ViewModels.Screens;
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
        private HrListViewModel Vm { get { return DataContext as HrListViewModel; } }

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
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Cut)
                Vm.Cut();
            if (e.Command == ApplicationCommands.Copy)
                Vm.Copy();
            if (e.Command == ApplicationCommands.Paste)
                Vm.Paste();
        }

    }
}