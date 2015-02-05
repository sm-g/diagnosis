using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.Common.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for TextBoxInfo.xaml
    /// </summary>
    public partial class TextBoxInfo : TextBox
    {
        public TextBoxInfo()
        {
            InitializeComponent();
        }

        public string InfoText
        {
            get { return (string)GetValue(InfoTextProperty); }
            set { SetValue(InfoTextProperty, value); }
        }

        public static readonly DependencyProperty InfoTextProperty =
            DependencyProperty.Register("InfoText", typeof(string), typeof(TextBoxInfo));
    }
}