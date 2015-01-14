using System.Windows;
using System.Windows.Input;

namespace Diagnosis.App.Windows
{
    public partial class MeasureEditorWindow : Window
    {
        public MeasureEditorWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                autocomplete.Focus();

            };
        }
    }
}