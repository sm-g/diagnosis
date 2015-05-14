using Diagnosis.Common;
using Diagnosis.Models;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Screens
{
    public class CritNavigator : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CritNavigator));
        private CriteriaItemViewModel _current;
        private ObservableCollection<Estimator> ests;
        private Action beforeInsert;
        private CritViewer viewer;
        private bool _noTop;

        public CritNavigator(CritViewer viewer, Action beforeInsert)
        {
            this.beforeInsert = beforeInsert;
            this.viewer = viewer;
            viewer.OpenedChanged += viewer_OpenedChanged;

            NoTopItems = true;
            TopItems = new ObservableCollection<CriteriaItemViewModel>();
            TopItems.CollectionChanged += (s, e) =>
            {
                NoTopItems = !TopItems.Any();
            };

            ests = new ObservableCollection<Estimator>();
            ests.CollectionChanged += nav_ests_CollectionChanged;
        }

        public event EventHandler<DomainEntityEventArgs> CurrentChanged;

        public event EventHandler<DomainEntityEventArgs> Navigating;

        public ObservableCollection<CriteriaItemViewModel> TopItems { get; private set; }

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

        public CriteriaItemViewModel Current
        {
            get
            {
                return _current;
            }
            private set
            {
                if (_current != value)
                {
                    _current = value;

                    OnCurrentCritChanged();

                    OnPropertyChanged(() => Current);
                    OnCurrentChanged(new DomainEntityEventArgs(value != null ? value.Crit : null));
                }
            }
        }

        public string CurrentTitle
        {
            get
            {
                if (Current == null)
                    return "";

                return GetCurrentPathDescription(Current.Crit);
            }
        }

        public static string GetCurrentPathDescription(ICrit current)
        {
            string delim = " \\ ";
            var sb = new StringBuilder();

            var est = current.GetEstimator();
            sb.Append(est);
            if (current is CriteriaGroup)
            {
                sb.Append(delim);
                sb.Append("группа");
                sb.Append(current as CriteriaGroup);
            }
            else if (current is Criterion)
            {
                var c = current as Criterion;
                sb.Append(delim);
                sb.Append("группа");
                sb.Append(c.Group);
                sb.Append(delim);
                sb.Append(c);
            }
            return sb.ToString();
        }

        public void NavigateTo(ICrit crit)
        {
            OnNavigating(new DomainEntityEventArgs(crit as IDomainObject));
            if (crit == null)
            {
                viewer.CloseAll();
                Current = null;
                return;
            }

            AddTopItemFor(crit);

            viewer.Open(crit);
            Current = FindItemVmOf(crit);
        }

        public void AddTopItemFor(ICrit crit)
        {
            var p = crit.GetEstimator();
            if (!ests.Contains(p))
            {
                ests.Add(p);
                var itemVm = new CriteriaItemViewModel(p, beforeInsert);
                TopItems.Add(itemVm);
            }
        }

        public void Remove(Estimator p)
        {
            if (ests.Remove(p))
            {
                var itemVm = FindItemVmOf(p);
                TopItems.Remove(itemVm);
            }
        }

        internal CriteriaItemViewModel FindItemVmOf(ICrit crit)
        {
            return TopItems.FindCritKeeperOf(crit);
        }

        protected virtual void OnCurrentChanged(DomainEntityEventArgs e)
        {
            logger.DebugFormat("Current is {0}", e.entity);

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

        private void viewer_OpenedChanged(object sender, CritViewer.OpeningEventArgs e)
        {
            Contract.Requires(e.entity is ICrit);
            logger.DebugFormat("{0} {1} {2}", e.action, e.entity.GetType().Name, e.entity);

            var crit = e.entity as ICrit;
            var est = crit as Estimator;

            if (e.action == CritViewer.OpeningAction.Open)
            {
                if (crit is Estimator)
                {
                    est.CriteriaGroupsChanged += est_GroupsChanged;
                    foreach (var item in est.CriteriaGroups)
                    {
                        item.CriteriaChanged += crGr_CriteriaChanged;
                    }
                }

                crit.PropertyChanged += crit_PropertyChanged;
            }
            else
            {
                if (crit is Estimator)
                {
                    est.CriteriaGroupsChanged -= est_GroupsChanged;
                    foreach (var item in est.CriteriaGroups)
                    {
                        item.CriteriaChanged -= crGr_CriteriaChanged;
                    }
                }
                crit.PropertyChanged -= crit_PropertyChanged;
            }
        }

        private void OnCurrentCritChanged()
        {
            if (Current == null)
                return;

            Current.IsSelected = true;
            Current.IsExpanded = true;
            Current.ExpandParents();

            // close nested
            var crit = Current.Crit;
            if (crit is Estimator)
            {
                viewer.OpenedCriteriaGroup = null;
            }
            else if (crit is CriteriaGroup)
            {
                viewer.OpenedCriterion = null;
            }

            OnPropertyChanged(() => CurrentTitle);
        }

        private void crit_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //OnPropertyChanged(() => CurrentTitle);
        }

        private void nav_ests_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var p = (Estimator)e.OldItems[0];

                if (viewer.OpenedEstimator == p)
                {
                    var near = ests.ElementNear(e.OldStartingIndex);
                    NavigateTo(near); //  рядом или null
                }
            }
        }

        private void est_GroupsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var crGr = (CriteriaGroup)e.NewItems[0];
                crGr.CriteriaChanged += crGr_CriteriaChanged;
                // при добавлении открываем его
                NavigateTo(crGr);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var crGr = (CriteriaGroup)e.OldItems[0];
                crGr.CriteriaChanged -= crGr_CriteriaChanged;

                // при удалении открытого открываем рядом с удаленным или est, если это был последний crGr
                if (viewer.OpenedCriteriaGroup == crGr)
                {
                    var near = viewer.OpenedEstimator.CriteriaGroups.ElementNear(e.OldStartingIndex);
                    if (near == null)
                    {
                        viewer.OpenedCriteriaGroup = null;
                        NavigateTo(viewer.OpenedEstimator);
                    }
                    else
                        NavigateTo(near);
                }
            }
        }

        private void crGr_CriteriaChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // при добавлении открываем его
                NavigateTo((Criterion)e.NewItems[0]);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var criterion = (Criterion)e.OldItems[0];
                // при удалении открытого открываем рядом или выше, если это был последний
                if (viewer.OpenedCriterion == criterion)
                {
                    viewer.Close(criterion);
                    ICrit near = viewer.OpenedCriteriaGroup.Criteria.ElementNear(e.OldStartingIndex);
                    NavigateTo(near ?? viewer.OpenedCriteriaGroup);
                }
            }
        }
    }
}