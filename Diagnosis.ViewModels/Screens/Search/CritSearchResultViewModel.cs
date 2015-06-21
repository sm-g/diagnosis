using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace Diagnosis.ViewModels.Screens
{
    public class CritSearchResultViewModel : AbstractSearchResultViewModel
    {
        private readonly Estimator est;

        public CritSearchResultViewModel(Dictionary<Criterion, IEnumerable<HealthRecord>> crHrs, IEnumerable<HealthRecord> topHrs, Estimator est)
        {
            this.est = est;

            // select patients from hrs
            var crps = (from pair in crHrs
                        let ps = from hr in pair.Value
                                 group hr by hr.GetPatient() into g
                                 orderby g.Key
                                 select g.Key
                        select new { Cr = pair.Key, Ps = ps }).ToDictionary(x => x.Cr, x => x.Ps);

            var patCrs = crps.ReverseManyToMany();

            var patHeadHrs = topHrs
                .GroupBy(x => x.GetPatient())
                .ToDictionary(x => x.Key, x => x.ToList());

            var vms = patCrs.Select(x => 
                new CritResultItemViewModel(x.Key, patHeadHrs.GetValueOrDefault(x.Key), x.Value));

            Patients = new ObservableCollection<IResultItem>(vms);

            Statistic = new CritStatistic(patCrs);

            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(Patients);
            view.GroupDescriptions.Add(new PropertyGroupDescription("Patient"));
        }

        public override object QuerySource
        {
            get { return est; }
        }

        protected override void RemoveDeleted(IHrsHolder h)
        {
            var holder = Patients.Cast<CritResultItemViewModel>().FirstOrDefault(x => x.Patient as IHrsHolder == h);
            if (holder != null)
            {
                Patients.Remove(holder);
            }
        }

        protected override void Export()
        {
        }
    }
}