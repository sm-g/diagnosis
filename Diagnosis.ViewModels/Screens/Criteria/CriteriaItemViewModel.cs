using Diagnosis.Common;
using Diagnosis.Models;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CriteriaItemViewModel : HierarchicalBase<CriteriaItemViewModel>
    {
        private bool _isHighlighted;

        public CriteriaItemViewModel(ICrit cr)
        {
            Contract.Requires(cr != null);

            Crit = cr;
            if (cr is Estimator)
            {
                var est = cr as Estimator;
                est.CriteriaGroups
                       .Select(i => new CriteriaItemViewModel(i))
                       .ForEach(v => Children.Add(v));

                est.CriteriaGroupsChanged += nested_IHrsHolders_Changed;
            }
            else if (cr is CriteriaGroup)
            {
                var cg = cr as CriteriaGroup;
                cg.Criteria
                       .Select(i => new CriteriaItemViewModel(i))
                       .ForEach(v => Children.Add(v));

                cg.CriteriaChanged += nested_IHrsHolders_Changed;
            }
            (cr as INotifyPropertyChanged).PropertyChanged += crit_PropertyChanged;

            IsExpanded = true;
        }

        public ICrit Crit { get; private set; }

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

        public bool IsHighlighted
        {
            get
            {
                return _isHighlighted;
            }
            set
            {
                if (_isHighlighted != value)
                {
                    _isHighlighted = value;
                    OnPropertyChanged(() => IsHighlighted);
                }
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
                        (Crit as Estimator).CriteriaGroupsChanged -= nested_IHrsHolders_Changed;
                    }
                    if (Crit is CriteriaGroup)
                    {
                        (Crit as CriteriaGroup).CriteriaChanged -= nested_IHrsHolders_Changed;
                    }
                    (Crit as INotifyPropertyChanged).PropertyChanged -= crit_PropertyChanged;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void nested_IHrsHolders_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (ICrit item in e.NewItems)
                {
                    var vm = new CriteriaItemViewModel(item);
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