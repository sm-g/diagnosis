using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.Client.App.Controls.FormParts
{
    /// <summary>
    /// Interaction logic for CategoryChooser.xaml
    /// </summary>
    public partial class CategoryChooser : UserControl
    {
        public CategoryChooser()
        {
            InitializeComponent();
        }

        public bool CategoryMultiSelection
        {
            get { return (bool)GetValue(CategoryMultiSelectionProperty); }
            set { SetValue(CategoryMultiSelectionProperty, value); }
        }

        public static readonly DependencyProperty CategoryMultiSelectionProperty =
            DependencyProperty.Register("CategoryMultiSelection", typeof(bool), typeof(CategoryChooser));
    }
}