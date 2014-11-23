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
