using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.App.Controls.Headers
{
    /// <summary>
    /// Interaction logic for Holder.xaml
    /// </summary>
    public partial class Holder : UserControl
    {
        public Holder()
        {
            InitializeComponent();
        }

        public Visibility TimeVisibility
        {
            get { return (Visibility)GetValue(TimeVisibilityProperty); }
            set { SetValue(TimeVisibilityProperty, value); }
        }

        public static readonly DependencyProperty TimeVisibilityProperty =
            DependencyProperty.Register("TimeVisibility", typeof(Visibility), typeof(Holder), new PropertyMetadata(Visibility.Collapsed));
    }
}