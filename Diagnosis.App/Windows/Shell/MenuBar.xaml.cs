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
    }
}