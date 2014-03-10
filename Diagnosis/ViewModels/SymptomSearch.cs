using Diagnosis.Models;
using System.Diagnostics.Contracts;

namespace Diagnosis.ViewModels
{
    public class SymptomSearch : HierarchicalSearch<SymptomViewModel>
    {
        /// <summary>
        /// Уровень (глубина), ниже которого и включая который искать симптомы.
        /// </summary>
        public int UpperLevel { get; set; }

        public SymptomSearch(SymptomViewModel parent, bool withNonCheckable = false, bool withChecked = false, bool allChildren = true, int upperLevel = 0)
            : base(parent, withNonCheckable, withChecked, allChildren)
        {
            UpperLevel = upperLevel;

            InitQuery();
        }

        protected override SymptomViewModel FromQuery(string query)
        {
            return new SymptomViewModel(new Symptom()
                {
                    Level = UpperLevel,
                    Title = query
                });
        }

        protected override bool CheckConditions(SymptomViewModel obj)
        {
            return base.CheckConditions(obj)
                   && (UpperLevel <= obj.Level);
        }
    }
}