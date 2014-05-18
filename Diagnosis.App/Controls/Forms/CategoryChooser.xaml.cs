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

namespace Diagnosis.App.Controls.Forms
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
