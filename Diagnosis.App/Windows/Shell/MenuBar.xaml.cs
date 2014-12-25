using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.App.Windows.Shell
{
    /// <summary>
    /// Interaction logic for MenuBar.xaml
    /// </summary>
    public partial class MenuBar : UserControl
    {
        public MenuBar()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new HelpWindow().Show();
        }
    }
}