using Diagnosis.Common.Presentation.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Screens
{
    /// <summary>
    /// Interaction logic for Patients.xaml
    /// </summary>
    public partial class Patients : UserControl
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Patients));

        public Patients()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                patients.Focus();
            };
        }

        private void dataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (dataGrid.SelectedCells.Count > 0)
                Keyboard.Focus(DataGridHelper.GetDataGridCell(dataGrid.SelectedCells[0]));
        }

        private void dataGrid_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            logger.DebugFormat("patients got kb focus, to datagrid={0}", e.NewFocus == patients);
        }
    }
}