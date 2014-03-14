using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class PatientSearch : SearchBase<PatientViewModel>
    {
        public PatientSearch(PatientsListVewModel patients)
            : base()
        {
            Contract.Requires(patients != null);
            Collection = patients.Patients;

            InitQuery();
        }

        protected override PatientViewModel FromQuery(string query)
        {
            var lfmNames = query.Split().ToList();

            while (lfmNames.Count < 3)
            {
                lfmNames.Add("");
            }
            return new PatientViewModel(new Patient()
                 {
                     LastName = lfmNames[0],
                     FirstName = lfmNames[1],
                     MiddleName = lfmNames[2],
                 });
        }
    }
}
