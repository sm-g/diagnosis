using Diagnosis.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for TileView.xaml
    /// </summary>
    public partial class TileView : UserControl
    {
        public TileView()
        {
            InitializeComponent();
        }


        private void explorer_Loaded(object sender, RoutedEventArgs e)
        {
            explorer.DataContext = new Diagnosis.ViewModels.HierarchicalExplorer<SymptomViewModel>(Diagnosis.DataCreator.Symptoms);
        }
    }
}