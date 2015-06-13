using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CritStatistic : StatisticBase
    {
        private ReadOnlyCollection<Patient> _pats;
        private Dictionary<Patient, IEnumerable<Criterion>> patCrs;

        public CritStatistic(Dictionary<Patient, IEnumerable<Criterion>> patCrs)
        {
            this.patCrs = patCrs;
            _pats = patCrs.Keys.ToList().AsReadOnly();
        }

        public override ReadOnlyCollection<Patient> Patients
        {
            get { return _pats; }
        }

        public override void Dispose()
        {
        }
    }
}