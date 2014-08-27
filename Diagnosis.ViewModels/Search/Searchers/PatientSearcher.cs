using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class PatientSearcher : ISimpleSearcher<PatientViewModel>
    {
        public IEnumerable<PatientViewModel> Collection { get; private set; }

        public PatientSearcher(IEnumerable<PatientViewModel> patients)
        {
            Contract.Requires(patients != null);
            Collection = patients;
        }

        public IEnumerable<PatientViewModel> Search(string query)
        {
            List<PatientViewModel> results = new List<PatientViewModel>();

            results.AddRange(Collection.Where(c => Filter(c as PatientViewModel, query)));

            return results;
        }

        protected bool Filter(PatientViewModel item, string query)
        {
            return item.patient.FullName.ToLower().Contains(query.ToLower());
        }

    }
}