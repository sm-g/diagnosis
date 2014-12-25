using System.IO;
using System.Text;
using System.Windows;

namespace Diagnosis.App.Windows
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                MemoryStream ms = new MemoryStream(UTF8Encoding.Default.GetBytes(Diagnosis.App.Properties.Resources.Help));
                rtb.Selection.Load(ms, DataFormats.Rtf);
            };
        }
    }
}