using Diagnosis.Common.Presentation.Controls;
using Diagnosis.ViewModels.Screens;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.Client.App.Screens
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
                Vm.PropertyChanged += Vm_PropertyChanged;

                ShowSortArrow();
            };
            Unloaded += (s, e) =>
            {
                Vm.PropertyChanged -= Vm_PropertyChanged;
            };
        }

        private PatientsListViewModel Vm { get { return DataContext as PatientsListViewModel; } }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Sorting" || e.PropertyName == "SortDirection")
            {
                ShowSortArrow();
            }
        }

        private void ShowSortArrow()
        {
            var col = dataGrid.Columns.Where(x => x.SortMemberPath == Vm.Sorting.ToString()).FirstOrDefault();
            if (col != null)
            {
                col.SortDirection = Vm.SortDirection;
            }
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