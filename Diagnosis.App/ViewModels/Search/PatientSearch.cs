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
            : base()
        {
            Contract.Requires(patients != null);
            Collection = patients;

            InitQuery();
        }

        protected override PatientViewModel FromQuery(string query)
        {
            var lfmNames = query.Split().ToList();

            // no patient without first and last names
            if (lfmNames.Count < 2 || lfmNames.Any(s => String.IsNullOrWhiteSpace(s)))
            {
                return null;
            }

            string middleName = lfmNames.Count < 3 ? null : lfmNames[2];
            return new PatientViewModel(new Patient(lfmNames[0], lfmNames[1], new DateTime(1980, 6, 15), middleName));
        }

        protected override bool Filter(PatientViewModel item, string query)
        {
            return item.ShortName.StartsWith(query, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
