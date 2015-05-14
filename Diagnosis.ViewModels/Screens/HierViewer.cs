using Diagnosis.Common;
using Diagnosis.Common.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    /// <summary>
    /// Хранит последние открытые сущности иерархии.
    ///
    /// AutoOpen:
    /// Если родительская сущность открыта повторно, открывает последнего открытого ребенка.
    ///
    /// При присвоении OpenedEntity сначала закрывается текущая открытая,
    /// затем меняется свойство OpenedEntity, потом открывается новая сущность.
    ///
    /// Перед открытием и после закрытия вызывается OpenedChanged.
    /// </summary>
    public class HierViewer<T1, T2, T3, I> : NotifyPropertyChangedBase
        where T1 : class, I
        where T2 : class, I
        where T3 : class, I
    {
        private T1 _openedRoot;
        private T2 _openedMiddle;
        private T3 _openedleaf;
        private Func<T2, IEnumerable<T3>> leavesOf;
        private Func<T1, IEnumerable<T2>> middlesOf;
        private Func<T2, T1> rootOf;
        private Func<T3, T2> middleOf;
        private Dictionary<T1, T2> rootMiddleMap;
        private Dictionary<T2, T3> middleLeafMap;
        private static List<T1> rootsOpenOrder = new List<T1>();

        public event EventHandler<OpeningEventArgs<I>> OpenedChanged;

        public HierViewer(Func<T2, T1> rootOf, Func<T3, T2> middleOf, Func<T1, IEnumerable<T2>> middlesOf, Func<T2, IEnumerable<T3>> leavesOf)
        {
            Contract.Requires(rootOf != null);
            Contract.Requires(middleOf != null);
            Contract.Requires(middlesOf != null);
            Contract.Requires(leavesOf != null);

            this.rootOf = rootOf;
            this.middleOf = middleOf;
            this.middlesOf = middlesOf;
            this.leavesOf = leavesOf;

            rootMiddleMap = new Dictionary<T1, T2>();
            middleLeafMap = new Dictionary<T2, T3>();
            rootsOpenOrder = new List<T1>(); // reset
        }

        public T1 OpenedRoot
        {
            get
            {
                return _openedRoot;
            }
            internal set
            {
                if (_openedRoot != value)
                {
                    if (_openedRoot != null)
                    {
                        OnRootClosed(_openedRoot);
                    }
                    _openedRoot = value;

                    OnPropertyChanged(() => OpenedRoot);
                    if (value != null)
                    {
                        OnRootOpened(value);
                        OnPropertyChanged(() => LastOpenedRoot);
                    }
                }
            }
        }

        public T2 OpenedMiddle
        {
            get
            {
                return _openedMiddle;
            }
            internal set
            {
                if (_openedMiddle != value)
                {
                    if (_openedMiddle != null)
                    {
                        OnMiddleClosed(_openedMiddle);
                    }

                    _openedMiddle = value;

                    OnPropertyChanged(() => OpenedMiddle);
                    if (value != null)
                    {
                        OnMiddleOpened(value);
                    }
                }
            }
        }

        public T3 OpenedLeaf
        {
            get
            {
                return _openedleaf;
            }
            internal set
            {
                if (_openedleaf != value)
                {
                    if (_openedleaf != null)
                    {
                        OnLeafClosed(_openedleaf);
                    }

                    _openedleaf = value;

                    OnPropertyChanged(() => OpenedLeaf);
                    if (value != null)
                    {
                        OnLeafOpened(value);
                    }
                }
            }
        }

        /// <summary>
        /// Открывать последнюю открытую дочернюю сущность при открытии родителя.
        /// </summary>
        public bool AutoOpenChild { get; set; }

        public static T1 LastOpenedRoot { get { return rootsOpenOrder.LastOrDefault(); } }

        public T3 GetLastOpenedFor(T2 middle)
        {
            T3 leaf;
            if (middleLeafMap.TryGetValue(middle, out leaf))
                return leaf;
            return null;
        }

        public T2 GetLastOpenedFor(T1 root)
        {
            T2 middle;
            if (rootMiddleMap.TryGetValue(root, out middle))
                return middle;
            return null;
        }

        public I GetLastOpenedFor(I node)
        {
            if (node is T1)
                return GetLastOpenedFor(node as T1);
            if (node is T2)
                return GetLastOpenedFor(node as T2);
            return default(I);
        }

        internal void Close(I node)
        {
            if (OpenedRoot.Equals(node))
                OpenedRoot = null;
            else if (OpenedMiddle.Equals(node))
                OpenedMiddle = null;
            else if (OpenedLeaf.Equals(node))
                OpenedLeaf = null;
            else
                throw new NotImplementedException();
        }

        internal void CloseAll()
        {
            OpenedRoot = null;
        }

        internal void OpenRoot(T1 T1)
        {
            OpenedRoot = T1;
        }

        internal void OpenMiddle(T2 T2)
        {
            OpenedRoot = rootOf(T2);
            OpenedMiddle = T2;
        }

        internal void OpenLeaf(T3 leaf)
        {
            OpenedRoot = rootOf(middleOf(leaf));
            OpenedMiddle = middleOf(leaf);
            OpenedLeaf = leaf;
        }

        internal void Open(I node)
        {
            if (node is T1)
                OpenRoot(node as T1);
            else if (node is T2)
                OpenMiddle(node as T2);
            else if (node is T3)
                OpenLeaf(node as T3);
            else
                throw new NotImplementedException();
        }

        internal void RemoveFromHistory(I node)
        {
            if (node is T1)
            {
                rootMiddleMap.Remove(node as T1);
                rootsOpenOrder.RemoveAll(x => x == node as T1);
            }
            else if (node is T2)
            {
                var p = rootMiddleMap.FirstOrDefault(x => x.Value == node as T2).Key;
                if (p != null)
                    rootMiddleMap.Remove(p);
                middleLeafMap.Remove(node as T2);
            }
            else if (node is T3)
            {
                var c = middleLeafMap.FirstOrDefault(x => x.Value == node as T3).Key;
                if (c != null)
                    middleLeafMap.Remove(c);
            }
            else
                throw new NotImplementedException();
        }

        private void OnRootOpened(T1 root)
        {
            var e = new OpeningEventArgs<I>(root, OpeningAction.Open);
            OnOpenedChanged(e);

            rootsOpenOrder.Add(root);

            if (AutoOpenChild)
            {
                T2 middle = GetLastOpenedFor(root);
                OpenedMiddle = middle ?? middlesOf(root).LastOrDefault();
            }
        }

        private void OnRootClosed(T1 root)
        {
            OpenedMiddle = null;

            var e = new OpeningEventArgs<I>(root, OpeningAction.Close);
            OnOpenedChanged(e);
        }

        private void OnMiddleOpened(T2 middle)
        {
            Contract.Requires(OpenedRoot == rootOf(middle));
            var e = new OpeningEventArgs<I>(middle, OpeningAction.Open);
            OnOpenedChanged(e);

            rootMiddleMap[OpenedRoot] = middle;

            if (AutoOpenChild)
            {
                T3 leaf = GetLastOpenedFor(middle);
                OpenedLeaf = leaf ?? leavesOf(middle).LastOrDefault();
            }
        }

        private void OnMiddleClosed(T2 middle)
        {
            OpenedLeaf = null;

            var e = new OpeningEventArgs<I>(middle, OpeningAction.Close);
            OnOpenedChanged(e);
        }

        private void OnLeafOpened(T3 leaf)
        {
            Contract.Requires(OpenedMiddle == middleOf(leaf));
            var e = new OpeningEventArgs<I>(leaf, OpeningAction.Open);
            OnOpenedChanged(e);

            middleLeafMap[OpenedMiddle] = leaf;
        }

        private void OnLeafClosed(T3 leaf)
        {
            var e = new OpeningEventArgs<I>(leaf, OpeningAction.Close);
            OnOpenedChanged(e);
        }

        protected virtual void OnOpenedChanged(OpeningEventArgs<I> e)
        {
            var h = OpenedChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
    }

    [Serializable]
    public class OpeningEventArgs<I> : EventArgs
    {
        public readonly OpeningAction action;
        public readonly I entity;

        [DebuggerStepThrough]
        public OpeningEventArgs(I entity, OpeningAction action)
        {
            this.action = action;
            this.entity = entity;
        }
    }

    public enum OpeningAction
    {
        Open, Close
    }
}