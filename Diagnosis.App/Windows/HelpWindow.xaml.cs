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
                var filename = "Help\\index.html";
                var path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, filename);
                webBrowser.Navigate(path);

                if (Left < 0)
                    Left = 0;
            };
        }
    }
}