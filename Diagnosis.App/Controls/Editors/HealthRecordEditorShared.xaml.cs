using Diagnosis.ViewModels.Search.Autocomplete;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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