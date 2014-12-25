using System.Diagnostics;
using System.Windows;

namespace Diagnosis.App.Windows
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();

            TraceListener debugListener = new TextBoxTraceListener(Log);
            Debug.Listeners.Add(debugListener);

            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            this.Height = SystemParameters.MaximizedPrimaryScreenHeight;
            this.Top = 0;
        }

        private void Log_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Log.ScrollToEnd();
        }
    }
}