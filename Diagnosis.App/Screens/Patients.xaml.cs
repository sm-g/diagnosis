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
using Diagnosis.ViewModels;
using Diagnosis.App.Controls;

namespace Diagnosis.App.Screens
{
    /// <summary>
    /// Interaction logic for Patients.xaml
    /// </summary>
    public partial class Patients : UserControl
    {
        public Patients()
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
