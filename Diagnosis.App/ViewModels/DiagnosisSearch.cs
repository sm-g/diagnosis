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
        public DiagnosisSearch(DiagnosisViewModel parent, bool withNonCheckable = false, bool withChecked = false, bool allChildren = true, int upperLevel = 0)
            : base(parent, withNonCheckable, withChecked, allChildren)
        {
            InitQuery();
        }

        protected override DiagnosisViewModel FromQuery(string query)
        {
            return new DiagnosisViewModel(new Diagnosis.Models.Diagnosis("12345", query));
        }
    }
}