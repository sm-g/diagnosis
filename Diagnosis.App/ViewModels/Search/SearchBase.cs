using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public abstract class SearchBase<T> : ViewModelBase, ISearchCheckable<T> where T : class, ICheckable
    {
        private string _query;
        private int _selectedIndex = -1;
        private ICommand _clear;
        private ICommand _searchCommand;
        private ICommand _selectCommand;
        private bool _searchActive;
        private bool _searchFocused;
        private bool _isResultsVisible;
        private bool _switchedOn;

        #region ISearch

        public event EventHandler ResultItemSelected;

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
                    IsResultsVisible = true;
                    OnPropertyChanged(() => Query);
                }
                MakeResults(_query);
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

        public ICommand ClearCommand
        {
            get
            {
                return _clear ?? (_clear = new RelayCommand(Clear,
                    () => Query != "" && SwitchedOn));
            }
        }

        public ICommand ToggleSearchCommand
        {
            get
            {
                return _searchCommand
                    ?? (_searchCommand = new RelayCommand(
                                          () =>
                                          {
                                              IsSearchActive = !IsSearchActive;
                                          },
                                          () => SwitchedOn
                       ));
            }
        }

        public ICommand SelectCommand
        {
            get
            {
                return _selectCommand ?? (_selectCommand = new RelayCommand(RaiseResultItemSelected,
                    () => SwitchedOn));
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
                    IsSearchFocused = value;
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
                    if (value)
                    {
                        IsResultsVisible = true;
                    }
                    OnPropertyChanged(() => IsSearchFocused);
                }
            }
        }

        public bool IsResultsVisible
        {
            get
            {
                return _isResultsVisible;
            }
            set
            {
                Console.Write("{0} -> {1}", _isResultsVisible, value);
                if (_isResultsVisible != value && (value == IsSearchFocused || IsSearchFocused)) // set to true only if IsSearchFocused
                {
                    Console.WriteLine(" YES");
                    _isResultsVisible = value;

                    OnPropertyChanged(() => IsResultsVisible);
                }
                else
                {
                    Console.WriteLine(" no");
                }
            }
        }

        public bool SwitchedOn
        {
            get
            {
                return _switchedOn;
            }
            set
            {
                if (_switchedOn != value)
                {
                    _switchedOn = value;
                    OnPropertyChanged(() => SwitchedOn);
                }
            }
        }

        public bool WithCreatingNew { get; set; }

        public void Clear()
        {
            Query = "";
        }

        public void RaiseResultItemSelected()
        {
            if (SwitchedOn)
            {
                var h = ResultItemSelected;
                if (h != null)
                {
                    h(this, new EventArgs());
                }
            }
            IsResultsVisible = false;
        }

        #endregion ISearch

        #region ISearchCheckable

        public bool WithNonCheckable { get; set; }

        public bool WithChecked { get; set; }

        public ObservableCollection<T> Results { get; protected set; }

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

        #endregion ISearchCheckable

        protected IEnumerable<T> Collection { get; set; }

        protected virtual void InitQuery()
        {
            Clear();
        }

        protected virtual T FromQuery(string query)
        {
            return null;
        }

        protected abstract bool Filter(T item, string query);

        private void MakeResults(string query)
        {
            Contract.Requires(query != null);

            // фильтруем коллекцию
            Results = new ObservableCollection<T>(
               Collection.Where(c => Filter(c, query)
                   && Filter(c)));

            if (WithCreatingNew
                && query != string.Empty
                && !Results.Any(c => Filter(c, query)))
            {
                // добавляем запрос к результатам
                Results.Add(FromQuery(query));
            }

            OnPropertyChanged(() => Results);

            if (Results.Count > 0)
                SelectedIndex = 0;
        }

        private bool Filter(T obj)
        {
            return (WithChecked || !obj.IsChecked)
                   && (WithNonCheckable || !obj.IsNonCheckable);
        }

        public SearchBase(bool withNonCheckable = false, bool withChecked = false, bool withCreatingNew = true, bool switchedOn = true)
        {
            WithNonCheckable = withNonCheckable;
            WithChecked = withChecked;
            WithCreatingNew = withCreatingNew;
            SwitchedOn = switchedOn;
        }
    }
}