using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class PatientSearch : SearchBase<PatientViewModel>
    {
        public PatientSearch(IEnumerable<PatientViewModel> patients)
            : base(withCreatingNew: false)
        {
            Contract.Requires(patients != null);
            Collection = patients;

            InitQuery();
        }

        protected override bool Filter(PatientViewModel item, string query)
        {
            return item.ShortName.StartsWith(query, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
