using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public abstract class CheckableBase : EditableBase, ICheckable
    {
        #region IEditable

        public override abstract string Name
        {
            get;
            set;
        }

        #endregion IEditable

        #region ICheckable

        private bool _isNonCheckable;
        private bool _isChecked;
        private ICommand _toggle;
        private bool _selected;

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
                    OnPropertyChanged(() => IsSelected);
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
                if (_isChecked != value && !IsNonCheckable)
                {
                    _isChecked = value;

                    OnPropertyChanged(() => IsChecked);

                    OnCheckedChanged();
                }
            }
        }

        public ICommand ToggleCommand
        {
            get
            {
                return _toggle
                    ?? (_toggle = new RelayCommand(
                        () =>
                        {
                            IsChecked = !IsChecked;
                            IsSelected = true;
                        }));
            }
        }

        #endregion ICheckableHierarchical

        protected abstract void OnCheckedChanged();
    }
}