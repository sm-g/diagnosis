using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class PatientSearcher : ISimpleSearcher<PatientViewModel>
    {
        public bool WithNonCheckable { get; private set; }

        public bool WithChecked { get; private set; }

        public bool WithCreatingNew { get; private set; }
        public bool AllChildren { get; private set; }

        public IEnumerable<PatientViewModel> Collection { get; private set; }

        public PatientSearcher(IEnumerable<PatientViewModel> patients, bool withNonCheckable = false, bool withChecked = false)
        {
            Contract.Requires(patients != null);
            Collection = patients;

            WithNonCheckable = withNonCheckable;
            WithChecked = withChecked;
            WithCreatingNew = false;
            AllChildren = false;
        }

        public IEnumerable<PatientViewModel> Search(string query)
        {
            List<PatientViewModel> results = new List<PatientViewModel>();

            results.AddRange(Collection.Where(c => Filter(c as PatientViewModel, query) && Filter(c as PatientViewModel)));

            return results;
        }

        protected bool Filter(PatientViewModel item, string query)
        {
            return item.patient.FullName.ToLower().Contains(query.ToLower());
        }

        private bool Filter(ICheckable obj)
        {
            return (WithChecked || !obj.IsChecked)
                   && (WithNonCheckable || !obj.IsNonCheckable);
        }
    }
}