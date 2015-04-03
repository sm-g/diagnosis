using Diagnosis.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Diagnosis.Common.Presentation.DebugTools
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow(LogTraceListener debugListener)
        {
            InitializeComponent();

            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var uiTaskFactory = new TaskFactory(uiScheduler);

            debugListener.LogEntries.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    // scroll to new
                    uiTaskFactory.StartNew(() =>
                        items.ScrollIntoView(e.NewItems[0]));
                }
            };
            debugListener.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "FilterOn")
                {
                    // scroll to selected
                    if (items.SelectedItem != null)
                        uiTaskFactory.StartNew(() =>
                            { items.ScrollIntoView(items.SelectedItem); });
                }
            };

            DataContext = debugListener;
            System.Diagnostics.Debug.Listeners.Add(debugListener);

            Loaded += (s, e) =>
            {
                if (this.Left == 0)
                    this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            };

            this.Height = SystemParameters.MaximizedPrimaryScreenHeight;
            this.Top = 0;
        }
    }
}