using Diagnosis.App.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

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
            explorer.DataContext = new Diagnosis.App.ViewModels.HierarchicalExplorer<SymptomViewModel>(Diagnosis.App.DataCreator.Symptoms);
        }
    }
}