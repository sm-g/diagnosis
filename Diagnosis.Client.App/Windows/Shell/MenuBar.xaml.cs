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
                view.Visibility = System.Windows.Visibility.Collapsed;
                view.IsEnabled = false;
#if !DEBUG
#endif
            };
        }
    }
}