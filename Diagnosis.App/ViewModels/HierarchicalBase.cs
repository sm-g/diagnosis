using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;

namespace Diagnosis.App.ViewModels
{
    /// <summary>
    /// Элемент иерархии.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class HierarchicalBase<T> : ViewModelBase, IHierarchical<T> where T : HierarchicalBase<T>
    {
        private T _parent;

        #region IHierarchical
        /// <summary>
        /// Возникает при добвляении или удалении ребенка.
        /// </summary>
        public event HierarchicalEventHandler<T> ChildrenChanged;
        /// <summary>
        /// Возникает при добавлении к родителю.
        /// </summary>
        public event HierarchicalEventHandler<T> ParentChanged;

        /// <summary>
        /// Родитель элемента. Элемент можеть быть ребенком только одного родителя.
        /// </summary>
        public T Parent
        {
            get
            {
                return _parent;
            }
            protected set
            {
                _parent = value;
                OnPropertyChanged("Parent");
            }
        }
        /// <summary>
        /// Дети на следующем уровне иерархии.
        /// </summary>
        public ObservableCollection<T> Children { get; private set; }
        /// <summary>
        /// Дети со всех уровней иерархии.
        /// </summary>
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
        /// <summary>
        /// Дети, у которых есть дети.
        /// </summary>
        public ObservableCollection<T> NonTerminalChildren
        {
            get
            {
                return new ObservableCollection<T>(Children.Where(i => !i.IsTerminal));
            }
        }
        /// <summary>
        /// Дети без детей, листья.
        /// </summary>
        public ObservableCollection<T> TerminalChildren
        {
            get
            {
                return new ObservableCollection<T>(Children.Where(i => i.IsTerminal));
            }
        }
        /// <summary>
        /// Элемент конечный в иерархии, лист.
        /// </summary>
        public bool IsTerminal
        {
            get
            {
                return Children.Count == 0;
            }
        }
        /// <summary>
        /// Корневой элемент, без родителя.
        /// </summary>
        public bool IsRoot
        {
            get
            {
                return Parent == null;
            }
        }

        /// <summary>
        /// Добавляет элемент к детям. У добавленного устанавливается родитель. Возвращает текущий элемент.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public T Add(T item)
        {
            if (item.Parent != null)
            {
                item.Parent.Remove(item);
            }

            item.Parent = (T)this;
            OnParentChanged(new HierarchicalEventAgrs<T>(item));
            Children.Add(item);
            return (T)this;
        }
        /// <summary>
        /// Добавляет несколько элементов к детям. У добавленных устанавливается родитель. Возвращает текущий элемент.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public T Add(IEnumerable<T> items)
        {
            foreach (var vm in items)
            {
                Add(vm);
            }
            return (T)this;
        }

        /// <summary>
        /// Добавляет элемент к детям, если его нет среди прямых детей или всех детей. Возвращает текущий элемент.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="checkAllChildren">Проверять ли наличие элемента на всех уровнях иерархии.</param>
        /// <returns></returns>
        public T AddIfNotExists(T item, bool checkAllChildren)
        {
            var query = checkAllChildren ? AllChildren : Children;

            if (query.SingleOrDefault(child => child == item) == null)
                Add(item);

            return (T)this;
        }

        /// <summary>
        /// Добавляет элемент к детям, если его нет среди прямых детей или всех детей. Возвращает текущий элемент.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="checkAllChildren">Проверять ли наличие элемента на всех уровнях иерархии.</param>
        /// <param name="equalsComparator">Делегат, отпределяющий совпадение элемента с детьми.</param>
        /// <returns></returns>
        public T AddIfNotExists(T item, bool checkAllChildren, Func<T, T, bool> equalsComparator)
        {
            var query = checkAllChildren ? AllChildren : Children;

            if (query.SingleOrDefault(child => equalsComparator(child, item)) == null)
                Add(item);

            return (T)this;
        }
        /// <summary>
        /// Удаляет элемент из детей, если он там был. Возвращает текущий элемент.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public T Remove(T item)
        {
            Children.Remove(item);
            return (T)this;
        }
        /// <summary>
        /// Удаляет элемент из иерархии.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual void Remove()
        {
            if (!IsRoot)
                Parent.Remove(this as T);
        }
        /// <summary>
        /// Удаляет несколько элементов из детей. Возвращает текущий элемент.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public T Remove(IEnumerable<T> items)
        {
            foreach (var vm in items)
            {
                Remove(vm);
            }
            return (T)this;
        }

        #endregion IHierarchical

        protected virtual void OnChildrenChanged(HierarchicalEventAgrs<T> e)
        {
            var h = ChildrenChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
        protected virtual void OnParentChanged(HierarchicalEventAgrs<T> e)
        {
            var h = ParentChanged;
            if (h != null)
            {
                h(this, e);
            }
        }

        private void OnChildAdded(T added)
        {
            OnPropertyChanged("TerminalChildren");
            OnPropertyChanged("NonTerminalChildren");
            OnPropertyChanged("IsTerminal");
            OnChildrenChanged(new HierarchicalEventAgrs<T>(added));
        }

        private void OnChildRemoved(T removed)
        {
            OnPropertyChanged("TerminalChildren");
            OnPropertyChanged("NonTerminalChildren");
            OnPropertyChanged("IsTerminal");
            OnChildrenChanged(new HierarchicalEventAgrs<T>(removed));
        }

        public HierarchicalBase()
        {
            Children = new ObservableCollection<T>();
            Children.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    OnChildAdded((T)e.NewItems[0]);
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    OnChildRemoved((T)e.OldItems[0]);
                }
            };
        }
    }

    public delegate void HierarchicalEventHandler<T>(object sender, HierarchicalEventAgrs<T> e);

    public class HierarchicalEventAgrs<T> : EventArgs
    {
        public T IHierarchical;

        [DebuggerStepThrough]
        public HierarchicalEventAgrs(T h)
        {
            IHierarchical = h;
        }
    }
}