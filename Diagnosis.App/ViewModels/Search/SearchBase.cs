using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class SearchBase<T> : ViewModelBase, ISearch<T> where T : class
    {
        internal readonly ISearcher<T> searcher;
        private string _query;
        private int _selectedIndex = -1;
        private ICommand _clear;
        private ICommand _searchCommand;
        private ICommand _resultsCommand;
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
                MakeResults();
            }
        }

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
                    () => !IsQueryEmpty && SwitchedOn));
            }
        }

        public ICommand ToggleSearchActiveCommand
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

        public ICommand ToggleResultsVisibleCommand
        {
            get
            {
                return _resultsCommand
                    ?? (_resultsCommand = new RelayCommand(
                                          () =>
                                          {
                                              IsResultsVisible = !IsResultsVisible;
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
                if (_isResultsVisible != value && (value == IsSearchFocused || IsSearchFocused)) // set to true only if IsSearchFocused
                {
                    _isResultsVisible = value;

                    OnPropertyChanged(() => IsResultsVisible);
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
                    h(this, EventArgs.Empty);
                }
            }
            IsResultsVisible = false;
        }

        #endregion ISearch

        private bool IsQueryEmpty
        {
            get { return string.IsNullOrWhiteSpace(Query); }
        }

        private void MakeResults()
        {
            if (IsQueryEmpty)
            {
                Results = new ObservableCollection<T>(searcher.Collection);
            }
            else
            {
                Results = new ObservableCollection<T>(searcher.Search(Query));
            }

            OnPropertyChanged(() => Results);

            if (Results.Count > 0)
                SelectedIndex = 0;
        }

        public SearchBase(ISearcher<T> searcher, bool switchedOn = true)
        {
            this.searcher = searcher;

            SwitchedOn = switchedOn;

            Clear();
        }
    }
}