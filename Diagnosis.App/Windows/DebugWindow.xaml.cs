using Diagnosis.Common;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

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

            //  TraceListener debugListener = new TextBoxTraceListener(Log);

            var debugListener = new LogTraceListener();
            debugListener.LogEntries.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    // scroll to new
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                    { items.ScrollIntoView(e.NewItems[0]); }));
                }
            };
            debugListener.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "FilterOn")
                {
                    // scroll to selected
                    if (items.SelectedItem != null)
                        Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                        { items.ScrollIntoView(items.SelectedItem); }));
                }
            };

            DataContext = debugListener;
            Debug.Listeners.Add(debugListener);

            Loaded += (s, e) =>
            {
                if (this.Left == 0)
                    this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            };
            Closing += (s, e) =>
            {
                Diagnosis.App.Properties.Settings.Default.DebugFilterOn = debugListener.FilterOn;

            };

            this.Height = SystemParameters.MaximizedPrimaryScreenHeight;
            this.Top = 0;
        }

        //private void Log_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        //{
        //    Action action = () =>
        //    {
        //         Log.ScrollToEnd();
        //    };
        //    Dispatcher.BeginInvoke(DispatcherPriority.Background, action);
        //}
    }

    public class LogTraceListener : TraceListener, INotifyPropertyChanged
    {
        private string _filter = Diagnosis.App.Properties.Settings.Default.DebugFilter ?? "";
        private bool _filterOn = Diagnosis.App.Properties.Settings.Default.DebugFilterOn;
        private int index = 0;

        public LogTraceListener()
        {
            this.Name = "LogTraceListener";
            LogEntries = new ObservableCollection<LogEntry>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LogEntry> LogEntries { get; set; }

        public string FilterContains
        {
            get
            {
                return _filter;
            }
            set
            {
                if (_filter != value)
                {
                    _filter = value;

                    Diagnosis.App.Properties.Settings.Default.DebugFilter = value;
                    ApplyFilter(FilterOn);
                    OnPropertyChanged("FilterContains");
                }
            }
        }

        public bool FilterOn
        {
            get
            {
                return _filterOn;
            }
            set
            {
                if (_filterOn != value)
                {
                    _filterOn = value;

                    ApplyFilter(value);
                    OnPropertyChanged("FilterOn");
                }
            }
        }

        public override void Write(string message)
        {
            LogEntries.Add(new LogEntry()
            {
                DateTime = DateTime.Now,
                Index = index++,
                Message = message,
                IsVisible = !FilterOn || FilterContains == "" || message.Contains(FilterContains)
            });
        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }

        [DebuggerStepThrough]
        protected void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (string name in propertyNames)
            {
                PropertyChangedEventHandler handler = this.PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

        private void ApplyFilter(bool on)
        {
            if (on)
                LogEntries.ForAll(l => l.IsVisible = l.Message.Contains(FilterContains));
            else
                LogEntries.ForAll(l => l.IsVisible = true);
        }
    }

    /// <summary>
    /// from http://stackoverflow.com/questions/16743804/implementing-a-log-viewer-with-wpf
    /// </summary>
    public class LogEntry : NotifyPropertyChangedBase
    {
        private bool _vis;

        public DateTime DateTime { get; set; }

        public int Index { get; set; }

        public string Message { get; set; }

        public bool IsVisible
        {
            get
            {
                return _vis;
            }
            set
            {
                if (_vis != value)
                {
                    _vis = value;
                    OnPropertyChanged(() => IsVisible);
                }
            }
        }
    }
}