using Diagnosis.Models;
using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Diagnosis.ViewModels.Search;
using EventAggregator;

namespace Diagnosis.ViewModels.Screens
{
    public class CritResultItemViewModel : ViewModelBase, IHolderKeeper, IResultItem
    {
        private readonly Patient _patient;
        private IEnumerable<Criterion> _crits;
        private IEnumerable<HealthRecord> _hrs;
        public CritResultItemViewModel(Patient p, IEnumerable<HealthRecord> topHrs, IEnumerable<Criterion> crits)
        {
            _patient = p;
            _crits = crits;
            _hrs = topHrs;
        }
        public Patient Patient { get { return _patient; } }
        public IEnumerable<Criterion> Criteria { get { return _crits; } }
        public IEnumerable<HealthRecord> TopHrs { get { return _hrs; } }

        public RelayCommand<Criterion> EditCriterionCommand
        {
            get
            {
                return new RelayCommand<Criterion>((cr) =>
                {
                    this.Send(Event.EditCrit, cr.AsParams(MessageKeys.Crit));
                });
            }
        }
        public IHrsHolder Holder
        {
            get { return _patient; }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                }

            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
