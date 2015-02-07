using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Diagnosis.App.Screens
{
    public partial class Sync : UserControl
    {
        public Sync()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                log.TextChanged += (s1, e1) =>
                {
                    Action action = () =>
                    {
                        log.ScrollToEnd();
                    };
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, action);
                };
            };
        }
    }
}