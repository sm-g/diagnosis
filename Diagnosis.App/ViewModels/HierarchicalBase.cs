using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public abstract class HierarchicalBase<T> : CheckableBase, IHierarchical<T>, IHierarchicalCheckable where T : HierarchicalBase<T>
    {
        #region IEditable

        public override abstract string Name
        {
            get;
            set;
        }

        #endregion IEditable

        #region IHierarchical

        private T _parent;

        public T Parent
        {
            get
            {
                return _parent;
            }
            protected set
            {
                _parent = value;
            }
        }

        public ObservableCollection<T> Children { get; private set; }

        public IEnumerable<T> AllChildren
        {
            get
            {
                List<T> result = Children.ToList();
                foreach (T child in Children)
                {
                    result.AddRange(child.AllChildren);
                }
                return result;
            }
        }

        public ObservableCollection<T> NonTerminalChildren
        {
            get
            {
                return new ObservableCollection<T>(Children.Where(i => !i.IsTerminal));
            }
        }

        public ObservableCollection<T> TerminalChildren
        {
            get
            {
                return new ObservableCollection<T>(Children.Where(i => i.IsTerminal));
            }
        }

        public bool IsTerminal
        {
            get
            {
                return Children.Count == 0;
            }
        }

        public bool IsRoot
        {
            get
            {
                return Parent == null;
            }
        }

        public T Add(T vm)
        {
            vm.Parent = (T)this;
            Children.Add(vm);
            OnChildAdded();
            return (T)this;
        }

        public T AddIfNotExists(T vm, bool checkAllChildren)
        {
            var query = checkAllChildren ? AllChildren : Children;

            if (query.SingleOrDefault(child => child == vm) == null)
                Add(vm);

            return (T)this;
        }

        public T Remove(T vm)
        {
            Children.Remove(vm);
            OnChildRemoved();
            return (T)this;
        }

        public void Delete()
        {
            // TODO
        }

        #endregion IHierarchical

        #region CheckableBase

        protected override void OnCheckedChanged()
        {
            if (!IsNonCheckable)
            {
                PropagateCheckedState(IsChecked);
                BubbleCheckedChildren();
            }
        }

        #endregion CheckableBase

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

        private void OnChildAdded()
        {
            OnPropertyChanged(() => TerminalChildren);
            OnPropertyChanged(() => IsTerminal);
        }

        private void OnChildRemoved()
        {
            OnPropertyChanged(() => TerminalChildren);
            OnPropertyChanged(() => NonTerminalChildren);
            OnPropertyChanged(() => IsTerminal);
        }

        public HierarchicalBase()
        {
            Children = new ObservableCollection<T>();
        }
    }
}