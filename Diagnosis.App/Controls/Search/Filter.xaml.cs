using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls.Search
{
    /// <summary>
    /// Interaction logic for Filter.xaml
    /// </summary>
    public partial class Filter : UserControl
    {
        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.Register("WatermarkText", typeof(string), typeof(Filter), new PropertyMetadata(null));

        public string WatermarkText
        {
            get { return (string)GetValue(WatermarkTextProperty); }
            set { SetValue(WatermarkTextProperty, value); }
        }

        public Filter()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                input.PreviewKeyDown += input_PreviewKeyDown;
            };
            Unloaded += (s, e) =>
            {
                input.PreviewKeyDown -= input_PreviewKeyDown;
            };
        }

        public event KeyEventHandler PreviewInputKeyDown;
        protected virtual void OnPreviewInputKeyDown(KeyEventArgs e)
        {
            var h = PreviewInputKeyDown;
            if (h != null)
            {
                h(this, e);
            }
        }

        void input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            OnPreviewInputKeyDown(e);
        }
    }
}