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
        }

        public event EventHandler<ObjectEventArgs> CurrentChanged;

        public event EventHandler<DomainEntityEventArgs> Navigating;

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

        public void NavigateTo(I node)
        {
            OnNavigating(new DomainEntityEventArgs(node));
            if (node == null)
            {
                viewer.CloseAll();
                Current = null;
                return;
            }

            AddRootItemFor(node);

            lastOpened = node;
            viewer.Open(node);
            Contract.Assume(node == lastOpened || viewer.AutoOpenChild);

            Current = FindItemVmOf(lastOpened);
        }

        public void RemoveRoot(T1 p)
        {
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
        protected abstract void OnRootClosed(T1 root);

        protected abstract void OnRootOpened(T1 root);

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
                if (node is T1)
                    OnRootOpened(node as T1);

                lastOpened = node;
                node.PropertyChanged += node_PropertyChanged;
            }
            else
            {
                if (node is T1)
                    OnRootClosed(node as T1);

                node.PropertyChanged -= node_PropertyChanged;
            }
        }

        private void node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(() => CurrentTitle);
        }

        private void navigator_roots_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var p = (T1)e.OldItems[0];

                if (viewer.OpenedRoot == p)
                {
                    var near = roots.ElementNear(e.OldStartingIndex);
                    NavigateTo(near); //  рядом или null
                }
            }
        }
    }
}