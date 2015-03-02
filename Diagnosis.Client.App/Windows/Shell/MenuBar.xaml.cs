using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.Client.App.Windows.Shell
{
    /// <summary>
    /// Interaction logic for MenuBar.xaml
    /// </summary>
    public partial class MenuBar : UserControl
    {
        public MenuBar()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
#if !DEBUG
                view.Visibility = System.Windows.Visibility.Collapsed;
                view.IsEnabled = false;
                metro.Visibility = System.Windows.Visibility.Collapsed;
                metro.IsEnabled = false;
                big.Visibility = System.Windows.Visibility.Collapsed;
                big.IsEnabled = false;
#endif
            };
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly()
                                          .GetName()
                                          .Version
                                          .ToString();
            MessageBox.Show(version, "Версия", MessageBoxButton.OK);
        }
    }
}