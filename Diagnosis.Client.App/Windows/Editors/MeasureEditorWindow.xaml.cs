using Diagnosis.ViewModels.Autocomplete;
using System;
using System.Windows;

namespace Diagnosis.Client.App.Windows
{
    public partial class MeasureEditorWindow : Window
    {
        public MeasureEditorWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                autocomplete.Focus();
                (DataContext as MeasureEditorViewModel).Autocomplete.InputEnded += Autocomplete_InputEnded;
            };
            Unloaded += (s, e) =>
            {
                (DataContext as MeasureEditorViewModel).Autocomplete.InputEnded -= Autocomplete_InputEnded;
            };
        }

        private void Autocomplete_InputEnded(object sender, EventArgs e)
        {
            value.Focus();
        }
    }
}