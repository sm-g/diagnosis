using Diagnosis.Models;
using Diagnosis.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class PatientSearch : SearchBase<PatientViewModel>
    {
        IPropertyManager propManager;

        public PatientSearch(PatientsListViewModel patients, IPropertyManager propManager)
            : base()
        {
            Contract.Requires(patients != null);
            Collection = patients.Patients;
            this.propManager = propManager;

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
            return new PatientViewModel(new Patient(lfmNames[0], lfmNames[1], new DateTime(1980, 6, 15), middleName), propManager);
        }
    }
}
