using Diagnosis.ViewModels.Screens;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Diagnosis.Client.App.Screens
{
    public partial class Vocabs : UserControl
    {
        public Vocabs()
        {
            InitializeComponent();

            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var uiTaskFactory = new TaskFactory(uiScheduler);

            Loaded += (s, e) =>
            {
                log.ScrollToEnd();

                log.TextChanged += (s1, e1) =>
                {
                    uiTaskFactory.StartNew(() =>
                    {
                        log.ScrollToEnd();
                    });
                };
            };

        }
    }
}