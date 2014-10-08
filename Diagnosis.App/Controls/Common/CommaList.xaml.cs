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
using System.Collections;

namespace Diagnosis.App.Controls
{
    /// <summary>
    /// Interaction logic for CommaList.xaml
    /// </summary>
    public partial class CommaList : UserControl
    {
        public CommaList()
        {
            InitializeComponent();
            if (Separator == null)
                Separator = new TextBlock() { Text = ", " };
        }

        public FrameworkElement Separator
        {
            get { return (FrameworkElement)GetValue(SeparatorProperty); }
            set { SetValue(SeparatorProperty, value); }
        }

        public static readonly DependencyProperty SeparatorProperty =
            DependencyProperty.Register("Separator", typeof(FrameworkElement), typeof(CommaList));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourseProperty); }
            set { SetValue(ItemsSourseProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourseProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CommaList));



        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(CommaList));

        
    }
}
