using Diagnosis.ViewModels.Screens;
using System.Windows.Controls;

namespace Diagnosis.Client.App.Controls.CardParts
{
    /// <summary>
    /// Interaction logic for HealthRecord.xaml
    /// </summary>
    public partial class HealthRecord : UserControl
    {
        ShortHealthRecordViewModel Vm { get { return DataContext as ShortHealthRecordViewModel; } }

        public HealthRecord()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
#if !DEBUG
                order.Visibility = System.Windows.Visibility.Collapsed;
#endif
                order.Visibility = System.Windows.Visibility.Collapsed;
            };
        }

        private void CheckBox_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // select by checkbox - focus on hr
            if (!(sender as CheckBox).IsChecked.Value)
                Vm.IsFocused = true;
        }
    }
}