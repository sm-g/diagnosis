using Diagnosis.App.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Screens
{
    public partial class Words : UserControl
    {
        public Words()
        {
            InitializeComponent();
        }

        private void dataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (dataGrid.SelectedCells.Count > 0)
                Keyboard.Focus(DataGridHelper.GetDataGridCell(dataGrid.SelectedCells[0]));
        }
    }
}