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

        protected override PatientViewModel Add(string query)
        {
            return new PatientViewModel(new Patient()
                 {
                     LastName = query,
                     MiddleName = "",
                     FirstName = ""
                 });
        }
    }
}
