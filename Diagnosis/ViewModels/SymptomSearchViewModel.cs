using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class SymptomSearchViewModel : SearchViewModel<SymptomViewModel>
    {
        public SymptomViewModel Parent { get; private set; }

        public bool WithGroups { get; set; }

        public bool WithChecked { get; set; }

        public bool AllChildren { get; set; }

        protected override void MakeResults(string query)
        {
            base.MakeResults(query);
            Results = new ObservableCollection<SymptomViewModel>(
                AllChildren
                ?
                Parent.AllChildren.Where(c => c.Representation.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)
                    && (WithChecked || !c.IsChecked)
                    && (WithGroups || !c.IsGroup))
                :
                Parent.Children.Where(c => c.Representation.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)
                    && (WithChecked || !c.IsChecked)
                    && (WithGroups || !c.IsGroup)));

            if (!Results.Any(c => c.Representation.Equals(query, StringComparison.InvariantCultureIgnoreCase)) &&
                query != string.Empty)
            {
                // добавляем запрос к результатам
                Results.Add(new SymptomViewModel(new Symptom()
                {
                    Parent = Parent.Symptom,
                    Title = query
                }));
            }

            OnPropertyChanged(() => Results);
            OnPropertyChanged(() => ResultsCount);

            if (ResultsCount > 0)
                SelectedIndex = 0;
        }

        public SymptomSearchViewModel(SymptomViewModel parent, bool withGroups = false, bool withChecked = false, bool allChildren = true)
            : base()
        {
            Contract.Requires(parent != null);

            Parent = parent;
            WithGroups = withGroups;
            WithChecked = withChecked;
            AllChildren = allChildren;

            Clear();
        }
    }
}