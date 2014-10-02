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
using System.Windows.Shapes;

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
    }
}
