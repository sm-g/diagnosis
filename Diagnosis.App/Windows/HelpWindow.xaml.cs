using System.IO;
using System.Text;
using System.Windows;
using Diagnosis.Common;

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
                MemoryStream ms = Diagnosis.App.Properties.Resources.Help.ToMemoryStream(Encoding.Default);
                rtb.Selection.Load(ms, DataFormats.Rtf);

                if (Left < 0)
                    Left = 0;
            };
        }
    }
}