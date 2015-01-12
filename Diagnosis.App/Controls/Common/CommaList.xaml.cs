using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.App.Controls
{
    /// <summary>
    /// Interaction logic for CommaList.xaml
    /// </summary>
    public partial class CommaList : UserControl
    {
        static FrameworkElement def = new TextBlock() { Text = ", " };
        public CommaList()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                if (Separator == null)
                    Separator = def;

            };
        }

        public FrameworkElement Separator
        {
            get { return (FrameworkElement)GetValue(SeparatorProperty); }
            set { SetValue(SeparatorProperty, value); }
        }

        public static readonly DependencyProperty SeparatorProperty =
            DependencyProperty.Register("Separator", typeof(FrameworkElement), typeof(CommaList)); // cannot set default FrameworkElement

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

        private void item_Loaded(object sender, RoutedEventArgs e)
        {
            var ch = (sender as DependencyObject).FindChild<ContentPresenter>("separator");
            if (ch != null)
                ch.Content = Separator.XamlClone();
        }
    }
}