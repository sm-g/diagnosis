using Diagnosis.Common;
using Diagnosis.Models;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;

using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public abstract class AbstractNavigatorViewModel<TItem, T1, T2, T3, I> : ViewModelBase
        where TItem : HierarchicalBase<TItem>//, IKeeper<I>
        where T1 : class, I
        where T2 : class, I
        where T3 : class, I
        where I : class, INotifyPropertyChanged, IDomainObject
    {
        protected HierViewer<T1, T2, T3, I> viewer;
        protected ObservableCollection<T1> roots;
        private static readonly ILog logger = LogManager.GetLogger(typeof(AbstractNavigatorViewModel<,,,,>));
        private TItem _curHolder;
        private I lastOpened;
        private bool _noTop;

        public AbstractNavigatorViewModel(HierViewer<T1, T2, T3, I> viewer)
        {
            this.viewer = viewer;
            viewer.OpenedChanged += viewer_OpenedChanged;

            NoTopItems = true;

            TopItems = new ObservableCollection<TItem>();
            TopItems.CollectionChanged += (s, e) =>
            {
                NoTopItems = !TopItems.Any();
            };

            roots = new ObservableCollection<T1>();
            roots.CollectionChanged += navigator_roots_CollectionChanged;

            NavigateToAdded = true;
            NavigateUpperOnRemoved = true;
        }

        public event EventHandler<ObjectEventArgs> CurrentChanged;

        public event EventHandler<DomainEntityEventArgs> Navigating;
        private bool inNavigating;

        public bool NoTopItems
        {
            get
            {
                return _noTop;
            }
            set
            {
                if (_noTop != value)
                {
                    _noTop = value;
                    OnPropertyChanged(() => NoTopItems);
                }
            }
        }

        public TItem Current
        {
            get
            {
                return _curHolder;
            }
            private set
            {
                if (_curHolder != value)
                {
                    _curHolder = value;

                    OnCurrentChanging();

                    OnPropertyChanged(() => Current);
                    OnCurrentChanged(new ObjectEventArgs(value != null ? value : null));
                }
            }
        }

        public string CurrentTitle
        {
            get
            {
                if (Current == null)
                    return "";

                return GetCurrentPathDescription(viewer, Current);
            }
        }

        public ObservableCollection<TItem> TopItems { get; private set; }

        public bool NavigateToAdded { get; set; }
        public bool NavigateUpperOnRemoved { get; set; }

        public void NavigateTo(I node)
        {
            OnNavigating(new DomainEntityEventArgs(node));
            if (node == null)
            {
                viewer.CloseAll();
                Current = null;
                return;
            }

            inNavigating = true;
            AddRootItemFor(node);
            inNavigating = false;

            lastOpened = node;
            viewer.Open(node);
            Contract.Assume(node == lastOpened || viewer.AutoOpenChild);

            Current = FindItemVmOf(lastOpened);
        }

        public void RemoveRoot(T1 p)
        {
            Contract.Ensures(viewer.OpenedRoot != p);

            if (roots.Remove(p))
            {
                var itemVm = FindItemVmOf(p);
                TopItems.Remove(itemVm);
            }
        }

        public abstract void AddRootItemFor(I node);

        protected internal abstract TItem FindItemVmOf(I node);

        protected abstract string GetCurrentPathDescription(HierViewer<T1, T2, T3, I> viewer, TItem current);
        protected abstract void CurrentChanging();
        protected abstract void OnRootRemoved(T1 root);

        protected abstract void OnRootAdded(T1 root);

        protected virtual void OnCurrentChanged(ObjectEventArgs e)
        {
            logger.DebugFormat("Current is {0}", e);

            var h = CurrentChanged;
            if (h != null)
            {
                h(this, e);
            }
        }

        protected virtual void OnNavigating(DomainEntityEventArgs e)
        {
            logger.DebugFormat("Navigating to {0}", e.entity);

            var h = Navigating;
            if (h != null)
            {
                h(this, e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                viewer.OpenedChanged -= viewer_OpenedChanged;
            }
            base.Dispose(disposing);
        }

        private void OnCurrentChanging()
        {
            if (Current == null)
                return;

            Current.IsSelected = true;
            // раскрываем открытый элемент дерева
            Current.IsExpanded = true;
            Current.ExpandParents();

            CurrentChanging();

            OnPropertyChanged(() => CurrentTitle);
        }
        protected void OnAdded(I node)
        {
            if (inNavigating) return;
            if (NavigateToAdded)
            {
                NavigateTo(node);
            }
        }
        /// <summary>
        /// при открытии подписываемся на измение коллекций доччерних сущностей
        /// сохраняем последний открытый
        ///
        /// </summary>
        private void viewer_OpenedChanged(object sender, OpeningEventArgs<I> e)
        {
            logger.DebugFormat("{0} {1} {2}", e.action, e.entity.GetType().Name, e.entity);

            var node = e.entity;

            if (e.action == OpeningAction.Open)
            {
                lastOpened = node;
                node.PropertyChanged += node_PropertyChanged;
            }
            else
            {
                node.PropertyChanged -= node_PropertyChanged;
            }
        }

        private void node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(() => CurrentTitle);
        }

        private void navigator_roots_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            T1 root;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    root = (T1)e.NewItems[0];
                    OnRootAdded(root);

                    OnAdded(root);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    root = (T1)e.OldItems[0];

                    if (viewer.OpenedRoot == root)
                    {
                        viewer.Close(root);
                        if (NavigateUpperOnRemoved)
                        {
                            var near = roots.ElementNear(e.OldStartingIndex);
                            NavigateTo(near); //  рядом или null
                        }
                    }
                    OnRootRemoved(root);
                    break;

            }
        }
    }
}