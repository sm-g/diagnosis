using System.Windows.Input;
using System.Diagnostics;

namespace Diagnosis.App.ViewModels
{
    public class CheckableBase : ViewModelBase, ICheckable
    {
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
                return _toggle
                    ?? (_toggle = new RelayCommand(
                        () =>
                        {
                            IsChecked = !IsChecked;
                            IsSelected = true;
                        }));
            }
        }

        public virtual void OnCheckedChanged()
        {
        }

        public virtual void OnSelectedChanged()
        {
        }

        #endregion ICheckable
    }
}