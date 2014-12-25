using System.Windows.Controls;

namespace Diagnosis.App.Screens
{
    public partial class Words : UserControl
    {
        public Words()
        {
            InitializeComponent();
            words.TreeView.SelectedItemChanged += (s, e) =>
            {
                //  scroll into view
            };
        }
    }
}