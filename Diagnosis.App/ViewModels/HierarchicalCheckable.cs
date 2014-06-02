using System.Linq;
using System.Windows.Input;
using System;

namespace Diagnosis.App.ViewModels
{
    public abstract class HierarchicalCheckable<T> : HierarchicalBase<T>, IHierarchicalCheckable<T> where T : HierarchicalCheckable<T>
    {
        readonly internal CheckableBase checkable;
        private bool _isFiltered;

        public event HierarhicalCheckableEventHandler<T> CheckedChanged;
        #region IHierarchicalCheckable

        public int CheckedChildren
        {
            get
            {
                if (IsTerminal)
                    return 0;
                return Children.Sum(s => s.CheckedChildren + (s.IsChecked ? 1 : 0));
            }
        }

        public bool IsFiltered
        {
            get
            {
                return _isFiltered;
            }
            set
            {
                if (_isFiltered != value)
                {
                    _isFiltered = value;
                    OnPropertyChanged(() => IsFiltered);
                }
            }
        }

        #endregion IHierarchicalCheckable
        #region ICheckable

        public bool IsSelected
        {
            get
            {
                return checkable.IsSelected;
            }
            set
            {
                checkable.IsSelected = value;
                OnPropertyChanged(() => IsSelected);
            }
        }

        public bool IsNonCheckable
        {
            get
            {
                return checkable.IsNonCheckable;
            }
            set
            {
                checkable.IsNonCheckable = value;
                OnPropertyChanged(() => IsNonCheckable);
            }
        }

        public bool IsChecked
        {
            get
            {
                return checkable.IsChecked;
            }
            set
            {
                checkable.IsChecked = value;
                OnPropertyChanged(() => IsChecked);
            }
        }
        public ICommand ToggleCommand
        {
            get
            {
                return checkable.ToggleCommand;
            }
        }

        public virtual void OnCheckedChanged()
        {
            if (!IsNonCheckable)
            {
                PropagateCheckedState(IsChecked);
                BubbleCheckedChildren();
                RaiseCheckedChanged(new HierarhicalCheckableEventArgs<T>(this));
            }
        }

        #endregion ICheckable

        public HierarchicalCheckable()
        {
            checkable = new CheckableBase();
            checkable.PropertyChanged += checkable_PropertyChanged;
        }

        private void checkable_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            if (e.PropertyName == "IsChecked")
            {
                OnCheckedChanged();
            }
        }

        private void PropagateCheckedState(bool newState)
        {
            if (newState && !IsRoot)
            {
                Parent.checkable.IsChecked = true;
            }

            if (!newState)
            {
                foreach (var item in Children)
                {
                    item.checkable.IsChecked = false;
                }
            }
        }

        private void BubbleCheckedChildren()
        {
            OnPropertyChanged(() => CheckedChildren);
            if (!IsRoot)
            {
                Parent.BubbleCheckedChildren();
            }
        }

        protected virtual void RaiseCheckedChanged(HierarhicalCheckableEventArgs<T> e)
        {
            var h = CheckedChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
    }


    public delegate void HierarhicalCheckableEventHandler<T>(object sender, HierarhicalCheckableEventArgs<T> e) where T : class;
    public class HierarhicalCheckableEventArgs<T> : EventArgs where T : class
    {
        public IHierarchicalCheckable<T> vm;

        public HierarhicalCheckableEventArgs(IHierarchicalCheckable<T> vm)
        {
            this.vm = vm;
        }
    }
}