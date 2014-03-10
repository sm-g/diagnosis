using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public abstract class CheckableBase : EditableBase, ICheckable
    {
        #region IEditable

        public override abstract string Name
        {
            get;
            set;
        }

        public override bool IsReady
        {
            get
            {
                return base.IsReady;
            }
        }

        #endregion IEditable

        #region ICheckable

        private bool _isNonCheckable;
        private bool _isChecked;
        private ICommand _toggle;

        public bool IsNonCheckable
        {
            get
            {
                return _isNonCheckable;
            }
            set
            {
                if (_isNonCheckable != value)
                {
                    _isNonCheckable = value;
                    IsChecked = !value;

                    OnPropertyChanged(() => IsNonCheckable);
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
                if (_isChecked != value)
                {
                    if (IsNonCheckable)
                    {
                        _isChecked = false;
                    }
                    else
                    {
                        _isChecked = value;
                    }

                    OnPropertyChanged(() => IsChecked);

                    OnCheckedChanged();
                }
            }
        }

        public void ToggleChecked()
        {
            IsChecked = !IsChecked;
        }

        public ICommand ToggleCommand
        {
            get
            {
                return _toggle
                    ?? (_toggle = new RelayCommand(
                        () =>
                        {
                            ToggleChecked();
                            IsSelected = true;
                        }));
            }
        }

        #endregion ICheckableHierarchical

        protected abstract void OnCheckedChanged();
    }
}