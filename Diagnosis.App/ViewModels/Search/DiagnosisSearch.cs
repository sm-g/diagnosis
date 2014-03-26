using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class DiagnosisSearch : HierarchicalSearch<DiagnosisViewModel>
    {
        public DiagnosisSearch(DiagnosisViewModel parent, bool withNonCheckable = false, bool withChecked = false, bool allChildren = true)
            : base(parent, withNonCheckable, withChecked, allChildren)
        {
            InitQuery();
        }

        protected override DiagnosisViewModel FromQuery(string query)
        {
            return new DiagnosisViewModel(new Diagnosis.Models.Diagnosis("12345", query, Parent.diagnosis));
        }

        protected override bool Filter(DiagnosisViewModel item, string query)
        {
            return item.Name.StartsWith(query, StringComparison.InvariantCultureIgnoreCase) ||
                item.Code.StartsWith(query, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}