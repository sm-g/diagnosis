using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        private string _query;
        private int _selectedIndex;

        public ObservableCollection<SymptomViewModel> Results { get; private set; }

        public SymptomViewModel Parent { get; private set; }

        public string Query
        {
            get
            {
                return _query;
            }
            set
            {
                if (_query != value)
                {
                    _query = value;
                    MakeResults(_query);
                    OnPropertyChanged(() => Query);
                }
            }
        }

        public SymptomViewModel SelectedItem
        {
            get
            {
                return Results[SelectedIndex];
            }
        }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged(() => SelectedIndex);
                    OnPropertyChanged(() => SelectedItem);
                }
            }
        }

        public int ResultsCount
        {
            get
            {
                return Results.Count;
            }
        }

        private void MakeResults(string query)
        {
            Contract.Requires(query != null);

            Results = new ObservableCollection<SymptomViewModel>(
                Parent.Children.Where(c => c.Name.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)));

            if (!Parent.Children.Any(c => c.Name.Equals(query, StringComparison.InvariantCultureIgnoreCase)) &&
                query != string.Empty)
            {
                Results.Add(new SymptomViewModel(new Symptom()
                {
                    Parent = Parent.Symptom,
                    Title = query
                }));
            }

            OnPropertyChanged(() => Results);
            OnPropertyChanged(() => ResultsCount);
            SelectedIndex = 0;
        }

        public SearchViewModel(SymptomViewModel parent)
        {
            Contract.Requires(parent != null);

            Results = new ObservableCollection<SymptomViewModel>();
            Parent = parent;
        }
    }
}