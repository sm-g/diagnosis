using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.Client.App.Controls.Editors
{
    /// <summary>
    /// Interaction logic for SexChooser.xaml
    /// </summary>
    public partial class SexChooser : UserControl
    {
        public SexChooser()
        {
            InitializeComponent();
        }

        public bool WithUnknown
        {
            get { return (bool)GetValue(WithUnknownProperty); }
            set { SetValue(WithUnknownProperty, value); }
        }

        public static readonly DependencyProperty WithUnknownProperty =
            DependencyProperty.Register("WithUnknown", typeof(bool), typeof(SexChooser), new PropertyMetadata(false));
    }
}