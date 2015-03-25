using Diagnosis.ViewModels.Screens;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Diagnosis.Client.App.Screens
{
    public partial class Sync : UserControl
    {
        public Sync()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                log.ScrollToEnd();

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