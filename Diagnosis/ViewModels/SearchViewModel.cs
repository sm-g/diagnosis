using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        private string _query;
        private ICommand _clear;
        private int _selectedIndex = -1;

        public ObservableCollection<SymptomViewModel> Results { get; private set; }

        public SymptomViewModel Parent { get; private set; }

        public bool WithGroups { get; set; }
        public bool WithChecked { get; set; }
        public bool AllChildren { get; set; }

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
                    OnPropertyChanged(() => Query);
                }
                MakeResults(_query);
            }
        }

        public SymptomViewModel SelectedItem
        {
            get
            {
                if (SelectedIndex != -1)
                    return Results[SelectedIndex];
                else
                    return null;
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

        public ICommand ClearCommand
        {
            get
            {
                return _clear ?? (_clear = new RelayCommand(Clear, () => Query != ""));
            }
        }

        public void Clear()
        {
            Query = "";
        }

        public void CheckSelectedItem()
        {
            if (SelectedItem != null)
            {
                var query = this.AllChildren ? Parent.AllChildren : Parent.Children;

                if (query.SingleOrDefault(child => child == SelectedItem) == null)
                    Parent.Add(SelectedItem);

                SelectedItem.ToggleChecked();
            }
        }

        private void MakeResults(string query)
        {
            Contract.Requires(query != null);

            Results = new ObservableCollection<SymptomViewModel>(
                AllChildren
                ?
                Parent.AllChildren.Where(c => c.Name.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)
                    && (WithChecked || !c.IsChecked)
                    && (WithGroups || !c.IsGroup))
                :
                Parent.Children.Where(c => c.Name.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)
                    && (WithChecked || !c.IsChecked)
                    && (WithGroups || !c.IsGroup)));

            if (!Results.Any(c => c.Name.Equals(query, StringComparison.InvariantCultureIgnoreCase)) &&
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

        public SearchViewModel(SymptomViewModel parent, bool withGroups = false, bool withChecked = false, bool allChildren = true)
        {
            Contract.Requires(parent != null);

            Results = new ObservableCollection<SymptomViewModel>();
            Parent = parent;
            WithGroups = withGroups;
            WithChecked = withChecked;
            AllChildren = allChildren;
            Clear();
        }
    }
}