using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class PatientSearchViewModel : SearchViewModel<PatientViewModel>
    {
        PatientsListVewModel patients;

        protected override void MakeResults(string query)
        {
            base.MakeResults(query);
            Results = new ObservableCollection<PatientViewModel>(
                patients.Patients.Where(p => p.Representation.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)));

            //if (!Results.Any(p => p.Representation.Equals(query, StringComparison.InvariantCultureIgnoreCase)) &&
            //    query != string.Empty)
            //{
            //    // добавляем запрос к результатам
            //    Results.Add(new PatientViewModel(new Patient()
            //    {
            //        LastName = query
            //    }));
            //}

            OnPropertyChanged(() => Results);
            OnPropertyChanged(() => ResultsCount);

            if (ResultsCount > 0)
                SelectedIndex = 0;
        }

        public PatientSearchViewModel(PatientsListVewModel patients)
            : base()
        {
            Contract.Requires(patients != null);
            this.patients = patients;

            Clear();
        }
    }
}
