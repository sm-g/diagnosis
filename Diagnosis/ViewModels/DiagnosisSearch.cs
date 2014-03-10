using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class DiagnosisSearch : SearchBase<DiagnosisViewModel>
    {
        public bool AllChildren { get; set; }

        public DiagnosisSearch(DiagnosisViewModel parent, bool withNonCheckable = false, bool withChecked = false, bool allChildren = true, int upperLevel = 0)
            : base(withNonCheckable, withChecked)
        {
            Contract.Requires(parent != null);

            AllChildren = allChildren;

            Collection = AllChildren ? parent.AllChildren : parent.Children;

            InitQuery();
        }

        protected override DiagnosisViewModel FromQuery(string query)
        {
            return new DiagnosisViewModel(new Diagnosis.Models.Diagnosis()
                {
                    Parent = null,
                    Title = query
                });
        }
    }
}