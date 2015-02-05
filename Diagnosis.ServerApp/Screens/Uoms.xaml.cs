using Diagnosis.Common.Presentation.Controls;
using Diagnosis.ViewModels.Screens;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.ServerApp.Screens
{
    public partial class Uoms : UserControl
    {
        public Uoms()
        {
            InitializeComponent();
            DataContext = new UomsListViewModel();
        }

        private void dataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (dataGrid.SelectedCells.Count > 0)
                Keyboard.Focus(DataGridHelper.GetDataGridCell(dataGrid.SelectedCells[0]));
        }
    }
}