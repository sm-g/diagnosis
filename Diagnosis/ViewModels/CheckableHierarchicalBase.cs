using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels
{
    public abstract class CheckableHierarchicalBase<T> : HierarchicalBase<T>, ICheckableHierarchical<T> where T : CheckableHierarchicalBase<T>
    {
        #region HierarchicalBase

        public override abstract string Name
        {
            get;
            set;
        }

        #endregion

        #region ICheckableHierarchical

        bool _isNonCheckable;
        private bool _isChecked;

        public bool IsNonCheckable
        {
            get
            {
                return
                    _isNonCheckable;
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
                    if (!IsNonCheckable)
                    {
                        PropagateCheckedState(value);
                        BubbleCheckedChildren();
                    }

                    OnCheckedChanged();
                }
            }
        }

        protected virtual void OnCheckedChanged()
        {
        }

        public void ToggleChecked()
        {
            IsChecked = !IsChecked;
        }

        public int CheckedChildren
        {
            get
            {
                if (IsTerminal)
                    return 0;
                return Children.Sum(s => s.CheckedChildren + (s.IsChecked ? 1 : 0));
            }
        }

        #endregion

        private void PropagateCheckedState(bool newState)
        {
            if (newState && !IsRoot)
            {
                Parent.IsChecked = true;
            }

            if (!newState)
            {
                foreach (var item in Children)
                {
                    item.IsChecked = false;
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
