using Diagnosis.App.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.App.Controls
{
    public partial class TilesViewer : UserControl
    {
        public TilesViewer()
        {
            InitializeComponent();
        }


        private void explorer_Loaded(object sender, RoutedEventArgs e)
        {
            explorer.DataContext = new HierarchicalExplorer<SymptomViewModel>(EntityManagers.SymptomsManager.Symptoms);
        }
    }
}