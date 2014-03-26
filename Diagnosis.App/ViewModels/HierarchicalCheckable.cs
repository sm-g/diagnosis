using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public abstract class HierarchicalCheckable<T> : HierarchicalBase<T>, IHierarchicalCheckable where T : HierarchicalCheckable<T>
    {
        readonly internal CheckableBase checkable;

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

        public virtual bool IsNonCheckable
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
    }
}