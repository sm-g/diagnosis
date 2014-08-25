using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class DiagnosisSearcher : ISimpleSearcher<DiagnosisViewModel>
    {
        IEnumerable<DiagnosisViewModel> checkedDiagnoses;

        public bool WithNonCheckable { get; set; }

        public bool WithChecked { get; set; }

        public bool WithCreatingNew { get; set; }
        public bool AllChildren { get; set; }

        public IEnumerable<DiagnosisViewModel> Collection { get; private set; }

        /// <summary>
        /// WithCreatingNew in settings will be set to false.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="settings"></param>
        public DiagnosisSearcher(DiagnosisViewModel parent, HierarchicalSearchSettings settings, IEnumerable<DiagnosisViewModel> checkedDiagnoses = null)
        {
            Contract.Requires(parent != null);
            Collection = settings.AllChildren ? parent.AllChildren : parent.Children;

            AllChildren = settings.AllChildren;
            WithNonCheckable = settings.WithNonCheckable;
            WithChecked = settings.WithChecked;
            WithCreatingNew = false;
            this.checkedDiagnoses = checkedDiagnoses;
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
                   && (WithNonCheckable || !obj.IsNonCheckable)
                   && (WithChecked || checkedDiagnoses == null|| !checkedDiagnoses.Contains(obj));
        }
    }
}