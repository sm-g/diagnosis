using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public abstract class SearchBase<T> : ViewModelBase, ISearch<T> where T : class, ICheckable, IEditable
    {
        private string _query;
        private int _selectedIndex = -1;
        private ICommand _clear;
        private ICommand _searchCommand;
        private ICommand _selectCommand;
        private bool _searchActive;
        private bool _searchFocused;

        public event EventHandler ResultItemSelected;

        public IEnumerable<T> Collection { get; protected set; }

        public bool WithNonCheckable { get; set; }

        public bool WithChecked { get; set; }

        public ObservableCollection<T> Results { get; protected set; }

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

        public T SelectedItem
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

        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand
                    ?? (_searchCommand = new RelayCommand(
                                          () =>
                                          {
                                              IsSearchActive = !IsSearchActive;
                                          }
                                          ));
            }
        }

        public ICommand SelectCommand
        {
            get
            {
                return _selectCommand ?? (_selectCommand = new RelayCommand(RaiseResultItemSelected));
            }
        }

        public bool IsSearchActive
        {
            get
            {
                return _searchActive;
            }
            set
            {
                if (_searchActive != value)
                {
                    _searchActive = value;
                    OnPropertyChanged(() => IsSearchActive);
                }
            }
        }

        public bool IsSearchFocused
        {
            get
            {
                return _searchFocused;
            }
            set
            {
                if (_searchFocused != value)
                {
                    _searchFocused = value;
                    OnPropertyChanged(() => IsSearchFocused);
                }
            }
        }

        public void Clear()
        {
            Query = "";
        }

        public void RaiseResultItemSelected()
        {
            var h = ResultItemSelected;
            if (h != null)
            {
                h(this, new EventArgs());
            }
        }

        private void MakeResults(string query)
        {
            Contract.Requires(query != null);

            Results = new ObservableCollection<T>(
               Collection.Where(c => c.Name.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)
                   && CheckConditions(c)));

            if (!Results.Any(c => c.Name.Equals(query, StringComparison.InvariantCultureIgnoreCase)) &&
                query != string.Empty)
            {
                // добавляем запрос к результатам
                Results.Add(FromQuery(query));
            }

            OnPropertyChanged(() => Results);
            OnPropertyChanged(() => ResultsCount);

            if (Results.Count > 0)
                SelectedIndex = 0;
        }

        protected virtual void InitQuery()
        {
            Clear();
        }

        protected virtual bool CheckConditions(T obj)
        {
            return (WithChecked || !obj.IsChecked)
                   && (WithNonCheckable || !obj.IsNonCheckable);
        }

        protected abstract T FromQuery(string query);

        public SearchBase(bool withNonCheckable = false, bool withChecked = false)
        {
            WithNonCheckable = withNonCheckable;
            WithChecked = withChecked;
        }
    }
}