using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;

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

            TraceListener debugListener = new MyTraceListener(Log);
            Debug.Listeners.Add(debugListener);

            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            this.Height = SystemParameters.MaximizedPrimaryScreenHeight;
            this.Top = 0;
        }

        private void Log_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Log.ScrollToEnd();
        }
    }
}
