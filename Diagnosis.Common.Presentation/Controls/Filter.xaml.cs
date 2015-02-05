using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.Common.Presentation.Controls.Search
{
    /// <summary>
    /// Interaction logic for Filter.xaml
    /// </summary>
    public partial class Filter : UserControl
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Filter));
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

        private void input_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            logger.DebugFormat("filter got kb focus, to input={0}", e.NewFocus == input);

        }
    }
}