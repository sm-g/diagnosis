using System;
using System.Windows.Input;
using System.Diagnostics;

namespace Diagnosis.ViewModels
{
    public abstract class CheckableBase : ViewModelBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(CheckableBase));

        private bool _isNonCheckable;
        private bool _isChecked;
        private bool _selected;

        public event EventHandler<CheckableEventArgs> CheckedChanged;
        public event EventHandler<CheckableEventArgs> SelectedChanged;

        public bool IsSelected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged("IsSelected");
                    // logger.DebugFormat("selected = {0}, {1}", value, this);
                    OnSelectedChanged();
                    if (SyncCheckedAndSelected)
                    {
                        IsChecked = value;
                    }
                }
            }
        }

        public virtual bool IsNonCheckable
        {
            get
            {
                return _isNonCheckable;
            }
            set
            {
                if (_isNonCheckable != value)
                {
                    IsChecked = !value;

                    _isNonCheckable = value;

                    OnPropertyChanged("IsNonCheckable");
                }
            }
        }

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (_isChecked != value && !IsNonCheckable)
                {
                    _isChecked = value;
                    OnPropertyChanged("IsChecked");
                    //  logger.DebugFormat("checked = {0}, {1}", value, this);
                    OnCheckedChanged();
                    if (SyncCheckedAndSelected)
                    {
                        IsSelected = value;
                    }
                }
            }
        }

        public ICommand ToggleCheckedCommand
        {
            get
            {
                return new RelayCommand(
                        () =>
                        {
                            IsChecked = !IsChecked;
                        });
            }
        }

        public bool SyncCheckedAndSelected { get; set; }

        protected virtual void OnCheckedChanged()
        {
            var h = CheckedChanged;
            if (h != null)
            {
                h(this, new CheckableEventArgs(this));
            }
        }

        protected virtual void OnSelectedChanged()
        {
            var h = SelectedChanged;
            if (h != null)
            {
                h(this, new CheckableEventArgs(this));
            }
        }
    }

    public class CheckableEventArgs : EventArgs
    {
        public readonly CheckableBase vm;

        [DebuggerStepThrough]
        public CheckableEventArgs(CheckableBase vm)
        {
            this.vm = vm;
        }
    }
}