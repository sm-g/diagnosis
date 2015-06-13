using Diagnosis.ViewModels.Controls;
using Diagnosis.ViewModels.Controls.Autocomplete;
using System;
using System.Linq;
using System.Windows.Controls;

namespace Diagnosis.Client.App.Windows.Editors
{
    /// <summary>
    /// Interaction logic for MeasureEditor.xaml
    /// </summary>
    public partial class MeasureEditor : UserControl
    {
        public MeasureEditor()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                autocomplete.Focus();
                (DataContext as MeasureEditorViewModel).Autocomplete.InputEnded += Autocomplete_InputEnded;
            };
            Unloaded += (s, e) =>
            {
                var vm = DataContext as MeasureEditorViewModel;
                if (vm != null)
                {
                    vm.Autocomplete.InputEnded -= Autocomplete_InputEnded;
                }
            };
        }

        private void Autocomplete_InputEnded(object sender, EventArgs e)
        {
            value.Focus();
        }
    }
}