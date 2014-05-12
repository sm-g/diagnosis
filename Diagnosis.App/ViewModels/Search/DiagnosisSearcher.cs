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

        public IEnumerable<DiagnosisViewModel> Collection { get; private set; }

        public DiagnosisSearcher(DiagnosisViewModel parent, bool withNonCheckable = false, bool withChecked = false, bool allChildren = true)
        {
            Contract.Requires(parent != null);
            Collection = allChildren ? parent.AllChildren : parent.Children;

            AllChildren = allChildren;
            WithNonCheckable = withNonCheckable;
            WithChecked = withChecked;
            WithCreatingNew = false;
        }

        public IEnumerable<DiagnosisViewModel> Search(string query)
        {
            List<DiagnosisViewModel> results = new List<DiagnosisViewModel>();

            results.AddRange(Collection.Where(diagnosis =>
                FilterCheckable(diagnosis) && Filter(diagnosis, query)));

            return results;
        }

        protected virtual bool Filter(DiagnosisViewModel item, string query)
        {
            return item.Name.ToLower().Contains(query.ToLower()) ||
               item.Code.StartsWith(query, StringComparison.InvariantCultureIgnoreCase);
        }

        protected bool FilterCheckable(ICheckable obj)
        {
            return (WithChecked || !obj.IsChecked)
                   && (WithNonCheckable || !obj.IsNonCheckable);
        }
    }
}