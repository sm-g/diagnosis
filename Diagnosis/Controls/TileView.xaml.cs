using System.Windows;
using System.Windows.Controls;

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
            explorer.DataContext = new Diagnosis.ViewModels.SymptomExplorerViewModel(Diagnosis.DataCreator.CreateSymptoms());
        }
    }
}