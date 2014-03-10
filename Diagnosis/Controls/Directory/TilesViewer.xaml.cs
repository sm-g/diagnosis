using Diagnosis.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace Diagnosis.Controls
{
    public partial class TilesViewer : UserControl
    {
        public TilesViewer()
        {
            InitializeComponent();
        }


        private void explorer_Loaded(object sender, RoutedEventArgs e)
        {
            explorer.DataContext = new Diagnosis.ViewModels.HierarchicalExplorer<SymptomViewModel>(Diagnosis.DataCreator.Symptoms);
        }
    }
}