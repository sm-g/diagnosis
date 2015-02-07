using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Diagnosis.Common.Presentation.DebugTools
{
    public class LogTraceListener : TraceListener, INotifyPropertyChanged
    {
        private string _filterContains;
        private bool _filterOn;
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
                return _filterContains;
            }
            set
            {
                if (_filterContains != value)
                {
                    _filterContains = value;

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
            Action action = () =>
            {
                LogEntries.Add(new LogEntry()
                {
                    DateTime = DateTime.Now,
                    Index = index++,
                    Message = message,
                    IsVisible = !FilterOn || FilterContains == "" || message.Contains(FilterContains)
                });
            };
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, action);

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
