using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public abstract class HierarchicalBaseViewModel<T> : ViewModelBase, IHierarchical<T> where T : HierarchicalBaseViewModel<T>
    {
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

        public abstract string Name
        {
            get;
            set;
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
            Contract.Requires(vm != null);

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

        public HierarchicalBaseViewModel()
        {
            Children = new ObservableCollection<T>();
        }
    }
}