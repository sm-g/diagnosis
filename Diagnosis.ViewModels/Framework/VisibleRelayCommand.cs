using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class VisibleRelayCommand : RelayCommand, INotifyPropertyChanged
    {
        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public VisibleRelayCommand(Action execute)
            : base(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public VisibleRelayCommand(Action execute, Func<bool> canExecute)
            : base(execute, canExecute)
        {
        }

        private bool _visible;

        public bool IsVisible
        {
            get
            {
                return _visible;
            }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public virtual event PropertyChangedEventHandler PropertyChanged;

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

        #endregion INotifyPropertyChanged Members
    }

    public class VisibleRelayCommand<T> : RelayCommand<T>, INotifyPropertyChanged
    {
        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public VisibleRelayCommand(Action<T> execute)
            : base(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public VisibleRelayCommand(Action<T> execute, Predicate<T> canExecute)
            : base(execute, canExecute)
        {
        }

        private bool _visible;

        public bool IsVisible
        {
            get
            {
                return _visible;
            }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public virtual event PropertyChangedEventHandler PropertyChanged;

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

        #endregion INotifyPropertyChanged Members
    }
}