using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public abstract class HierarchicalBase<T> : ViewModelBase, IHierarchical<T> where T : HierarchicalBase<T>
    {
        private T _parent;

        #region IHierarchical

        public event EventHandler ChildrenChanged;

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

        public T Add(IEnumerable<T> vms)
        {
            foreach (var vm in vms)
            {
                Add(vm);
            }
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

        protected virtual void OnChildrenChanged()
        {
            var h = ChildrenChanged;
            if (h != null)
            {
                h(this, new EventArgs());
            }
        }

        private void OnChildAdded()
        {
            OnPropertyChanged(() => TerminalChildren);
            OnPropertyChanged(() => IsTerminal);
            OnChildrenChanged();
        }

        private void OnChildRemoved()
        {
            OnPropertyChanged(() => TerminalChildren);
            OnPropertyChanged(() => NonTerminalChildren);
            OnPropertyChanged(() => IsTerminal);
            OnChildrenChanged();
        }

        public HierarchicalBase()
        {
            Children = new ObservableCollection<T>();
        }
    }
}