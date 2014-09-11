using System;
using System.Windows.Input;
using System.Diagnostics;

namespace Diagnosis.ViewModels
{
    public abstract class CheckableBase : ViewModelBase, ICheckable
    {
        #region ICheckable

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
                    Debug.Print("is selected = {0}", value);
                    OnSelectedChanged();
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
                    Debug.Print("is checked = {0}", value);
                    OnCheckedChanged();
                }
            }
        }

        public ICommand ToggleCommand
        {
            get
            {
                return new RelayCommand(
                        () =>
                        {
                            IsChecked = !IsChecked;
                            IsSelected = true;
                        });
            }
        }

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

        #endregion ICheckable
    }

    [Serializable]
    public class CheckableEventArgs : EventArgs
    {
        public readonly ICheckable vm;

        [DebuggerStepThrough]
        public CheckableEventArgs(ICheckable vm)
        {
            this.vm = vm;
        }
    }
}