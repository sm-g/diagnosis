using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class DiagnosisSearcher : ISearcher<DiagnosisViewModel>
    {
        public bool WithNonCheckable { get; set; }

        public bool WithChecked { get; set; }

        public bool WithCreatingNew { get; set; }
        public bool AllChildren { get; set; }

        public IEnumerable<DiagnosisViewModel> Collection { get; set; }

        public DiagnosisSearcher(DiagnosisViewModel parent, bool withNonCheckable = false, bool withChecked = false, bool allChildren = true)
        {
            Contract.Requires(parent != null);
            Collection = AllChildren ? parent.AllChildren : parent.Children;

            AllChildren = allChildren;
            WithNonCheckable = withNonCheckable;
            WithChecked = withChecked;
            WithCreatingNew = false;
        }

        public IEnumerable<DiagnosisViewModel> Search(string query)
        {
            List<DiagnosisViewModel> results = new List<DiagnosisViewModel>();

            results.AddRange(Collection.Where(c => Filter(c as DiagnosisViewModel, query) && Filter(c as DiagnosisViewModel)));

            return results;
        }

        protected bool Filter(DiagnosisViewModel item, string query)
        {
            return item.Name.ToLower().Contains(query.ToLower()) ||
               item.Code.StartsWith(query, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool Filter(ICheckable obj)
        {
            return (WithChecked || !obj.IsChecked)
                   && (WithNonCheckable || !obj.IsNonCheckable);
        }
    }
}