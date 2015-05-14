using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Screens
{
    public class CritNavigator : AbstractNavigatorViewModel<CriteriaItemViewModel, Estimator, CriteriaGroup, Criterion, ICrit>
    {
        private Action beforeInsert;

        public CritNavigator(HierViewer<Estimator, CriteriaGroup, Criterion, ICrit> viewer, Action beforeInsert)
            : base(viewer)
        {
            this.beforeInsert = beforeInsert;
        }

        public override void AddRootItemFor(ICrit crit)
        {
            var p = crit.GetEstimator();
            if (!roots.Contains(p))
            {
                roots.Add(p);
                var itemVm = new CriteriaItemViewModel(p, beforeInsert);
                TopItems.Add(itemVm);
            }
        }

        protected override string GetCurrentPathDescription(HierViewer<Estimator, CriteriaGroup, Criterion, ICrit> viewer, CriteriaItemViewModel current)
        {
            string delim = " \\ ";
            var sb = new StringBuilder();

            var crit = current.Crit;
            var est = crit.GetEstimator();
            sb.Append(est);
            if (crit is CriteriaGroup)
            {
                sb.Append(delim);
                sb.Append("группа");
                sb.Append(crit as CriteriaGroup);
            }
            else if (crit is Criterion)
            {
                var c = crit as Criterion;
                sb.Append(delim);
                sb.Append("группа");
                sb.Append(c.Group);
                sb.Append(delim);
                sb.Append(c);
            }
            return sb.ToString();
        }
        protected override void CurrentChanging()
        {
            // close nested
            var crit = Current.Crit;
            if (crit is Estimator)
                viewer.OpenedMiddle = null;
            else if (crit is CriteriaGroup)
                viewer.OpenedLeaf = null;
        }

        protected internal override CriteriaItemViewModel FindItemVmOf(ICrit crit)
        {
            return TopItems.FindCritKeeperOf(crit);
        }

        protected override void OnRootClosed(Estimator est)
        {
            est.CriteriaGroupsChanged -= est_GroupsChanged;
            foreach (var item in est.CriteriaGroups)
            {
                item.CriteriaChanged -= crGr_CriteriaChanged;
            }
        }

        protected override void OnRootOpened(Estimator est)
        {
            est.CriteriaGroupsChanged += est_GroupsChanged;
            foreach (var item in est.CriteriaGroups)
            {
                item.CriteriaChanged += crGr_CriteriaChanged;
            }
        }

        private void est_GroupsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var crGr = (CriteriaGroup)e.NewItems[0];
                crGr.CriteriaChanged += crGr_CriteriaChanged;
                NavigateTo(crGr);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var crGr = (CriteriaGroup)e.OldItems[0];
                crGr.CriteriaChanged -= crGr_CriteriaChanged;

                if (viewer.OpenedMiddle == crGr)
                {
                    viewer.Close(crGr);
                    ICrit near = viewer.OpenedRoot.CriteriaGroups.ElementNear(e.OldStartingIndex);
                    NavigateTo(near ?? viewer.OpenedRoot);
                }
            }
        }

        // при добавлении открываем его
        // при удалении открытого открываем рядом или выше, если это был последний
        private void crGr_CriteriaChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                NavigateTo((Criterion)e.NewItems[0]);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var criterion = (Criterion)e.OldItems[0];
                if (viewer.OpenedLeaf == criterion)
                {
                    viewer.Close(criterion);
                    ICrit near = viewer.OpenedMiddle.Criteria.ElementNear(e.OldStartingIndex);
                    NavigateTo(near ?? viewer.OpenedMiddle);
                }
            }
        }
    }
}