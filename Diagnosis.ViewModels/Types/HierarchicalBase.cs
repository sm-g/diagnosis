using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Diagnosis.Common;
using Diagnosis.Common.Types;

namespace Diagnosis.ViewModels
{
    /// <summary>
    /// Элемент иерархии.
    /// </summary>
    public abstract class HierarchicalBase<T> : CheckableBase where T : HierarchicalBase<T>
    {
        private T _parent;
        private bool _isFiltered;
        private bool _isExpanded;

        #region IHierarchical
        /// <summary>
        /// Возникает при добвляении или удалении ребенка.
        /// </summary>
        public event EventHandler<HierarchicalEventAgrs<T>> ChildrenChanged;
        /// <summary>
        /// Возникает при добавлении к родителю. (Не бывает удаления из родителя — всегда есть корневой элемент).
        /// </summary>
        public event EventHandler<HierarchicalEventAgrs<T>> ParentChanged;

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
                OnPropertyChanged(() => Level);
                OnParentChanged(new HierarchicalEventAgrs<T>((T)this));
            }
        }
        /// <summary>
        /// Дети на следующем уровне иерархии.
        /// </summary>
        public AsyncObservableCollection<T> Children { get; private set; }
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
        /// Элемент конечный в иерархии, лист, без детей.
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
        /// Элемент развернут.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }
        /// <summary>
        /// Уровень иерархии, 0 у корня.
        /// </summary>
        public int Level
        {
            get
            {
                var hb = this;
                int level = 0;
                while (!hb.IsRoot)
                {
                    hb = hb.Parent;
                    level++;
                }
                return level;
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
        /// Удаляет элемент из детей, если он там был. Возвращает текущий элемент.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public T Remove(T item)
        {
            if (Children.Remove(item))
                item.Parent = null;
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
            foreach (var vm in items.ToList())
            {
                Remove(vm);
            }
            return (T)this;
        }

        public void ExpandParents()
        {
            if (Parent != null)
            {
                Parent.IsExpanded = true;
                Parent.ExpandParents();
            }
        }

        /// <summary>
        /// Применяет действие к элементу и всем его детям.
        /// </summary>
        public void ForBranch(Action<HierarchicalBase<T>> action)
        {
            action(this);
            foreach (var item in this.AllChildren)
            {
                action(item);
            }
        }

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


        #endregion IHierarchical

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
                    OnPropertyChanged("IsFiltered");
                }
            }
        }

        #endregion IHierarchicalCheckable

        #region CheckableBase

        protected override void OnCheckedChanged()
        {
            base.OnCheckedChanged();

            if (!IsNonCheckable)
            {
                PropagateCheckedState(IsChecked);
                BubbleCheckedChildren();
            }
        }

        protected override void OnSelectedChanged()
        {
            base.OnSelectedChanged();

            if (IsSelected && !IsRoot)
                Parent.IsExpanded = true;
        }

        #endregion CheckableBase

        public HierarchicalBase()
        {
            Children = new AsyncObservableCollection<T>();
            Children.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    var item = (T)e.NewItems[0];
                    if (item.Parent != this)
                    {
                        item.Parent = (T)this;
                    }
                    OnChildAdded((T)e.NewItems[0]);
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    OnChildRemoved((T)e.OldItems[0]);
                }
            };
        }

        /// <summary>
        /// If newState true, checks all parents.
        /// Else unckecks all children.
        /// </summary>
        /// <param name="newState"></param>
        private void PropagateCheckedState(bool newState)
        {
            // check parent
            if (newState && !IsRoot)
            {
                Parent.IsChecked = true;
            }
            // uncheck children
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
            OnPropertyChanged("CheckedChildren");
            if (!IsRoot)
            {
                Parent.BubbleCheckedChildren();
            }
        }

    }

    public class HierarchicalEventAgrs<T> : EventArgs where T : HierarchicalBase<T>
    {
        public readonly T IHierarchical;

        [DebuggerStepThrough]
        public HierarchicalEventAgrs(T h)
        {
            IHierarchical = h;
        }
    }
}