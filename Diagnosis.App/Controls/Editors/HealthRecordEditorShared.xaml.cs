using Diagnosis.ViewModels.Search.Autocomplete;
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

namespace Diagnosis.App.Controls.Editors
{
    public partial class HealthRecordEditorShared : UserControl
    {
        public HealthRecordEditorShared()
        {
            InitializeComponent();
#if !DEBUG
            offsetSettings.Visibility = System.Windows.Visibility.Collapsed;
#endif
        }

        private void hr_grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            hr_grid.Focus();
        }
        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            (autocomplete.DataContext as Autocomplete).OnDrop(e);
        }
    }
}
