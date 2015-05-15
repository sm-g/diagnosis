using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CriteriaItemViewModel : HierarchicalBase<CriteriaItemViewModel>, ICritKeeper
    {
        private readonly ICrit crit;
        private VisibleRelayCommand _addCriterion;
        private VisibleRelayCommand _addCrGroup;
        private Action beforeInsert;

        public CriteriaItemViewModel(ICrit crit, Action beforeInsert)
        {
            Contract.Requires(crit != null);

            this.beforeInsert = beforeInsert;
            this.crit = crit;
            if (crit is Estimator)
            {
                var est = crit as Estimator;
                est.CriteriaGroups
                       .Select(i => new CriteriaItemViewModel(i, beforeInsert))
                       .ForEach(v => Children.Add(v));

                est.CriteriaGroupsChanged += nested_ICrit_Changed;
            }
            else if (crit is CriteriaGroup)
            {
                var cg = crit as CriteriaGroup;
                cg.Criteria
                       .Select(i => new CriteriaItemViewModel(i, beforeInsert))
                       .ForEach(v => Children.Add(v));

                cg.CriteriaChanged += nested_ICrit_Changed;
            }
            crit.PropertyChanged += crit_PropertyChanged;

            IsExpanded = true;
        }

        public ICrit Crit { get { return crit; } }

        public RelayCommand OpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.OpenCrit, Crit.AsParams(MessageKeys.Crit));
                });
            }
        }

        public VisibleRelayCommand AddCriterionCommand
        {
            get
            {
                return _addCriterion ?? (_addCriterion = new VisibleRelayCommand(() =>
                {
                    beforeInsert();
                    (Crit as CriteriaGroup).AddCriterion();
                })
                {
                    IsVisible = Crit is CriteriaGroup
                });
            }
        }

        public VisibleRelayCommand AddCritGroupCommand
        {
            get
            {
                return _addCrGroup ?? (_addCrGroup = new VisibleRelayCommand(() =>
                {
                    beforeInsert();
                    (Crit as Estimator).AddCriteriaGroup();
                })
                {
                    IsVisible = Crit is Estimator
                });
            }
        }

        public VisibleRelayCommand InsertCommand
        {
            get
            {
                return Crit is Estimator ? AddCritGroupCommand : AddCriterionCommand;
            }
        }

        public RelayCommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.OpenCrit, Crit.AsParams(MessageKeys.Crit));
                });
            }
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.DeleteCrit, Crit.AsParams(MessageKeys.Crit));
                }, () => Crit.IsEmpty());
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.GetType(), Crit);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (Crit is Estimator)
                    {
                        (Crit as Estimator).CriteriaGroupsChanged -= nested_ICrit_Changed;
                    }
                    if (Crit is CriteriaGroup)
                    {
                        (Crit as CriteriaGroup).CriteriaChanged -= nested_ICrit_Changed;
                    }
                    Crit.PropertyChanged -= crit_PropertyChanged;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void nested_ICrit_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (ICrit item in e.NewItems)
                {
                    var vm = new CriteriaItemViewModel(item, beforeInsert);
                    this.Add(vm);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (ICrit item in e.OldItems)
                {
                    var vm = Children.Where(w => w.Crit == item).FirstOrDefault();
                    this.Remove(vm);
                    vm.Dispose();
                }
            }
        }

        private void crit_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(() => Crit); // refresh text binding
        }
    }
}