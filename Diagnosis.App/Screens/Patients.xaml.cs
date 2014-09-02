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
    public partial class Patients : Page
    {
        IInputElement LastFocused = null;
        bool isloaded;

        public Patients()
        {
            InitializeComponent();
        }

        private void patientsControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedCells.Count > 0)
                Keyboard.Focus(DataGridHelper.GetDataGridCell(dataGrid.SelectedCells[0]));

            if (isloaded)
                return;
            isloaded = true;

            this.NavigationService.Navigating += NavigationService_Navigating;
        }

        void NavigationService_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Content == this)
                FocusManager.SetFocusedElement(this, LastFocused);
            else
                LastFocused = FocusManager.GetFocusedElement(patientsControl);
        }
    }
}
