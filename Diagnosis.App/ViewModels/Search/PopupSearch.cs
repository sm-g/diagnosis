using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class PopupSearch<T> : ViewModelBase where T : class
    {
        internal readonly ISimpleSearcher<T> searcher;
        readonly Action<T> onSelected;

        private string _query;
        private int _selectedIndex = -1;
        private ICommand _clear;
        private ICommand _searchCommand;
        private ICommand _resultsCommand;
        private ICommand _selectCommand;
        private bool _searchActive;
        private bool _searchFocused;
        private bool _isResultsVisible;
        private bool _resultsOnQueryChanges;
        private bool _switchedOn;

        #region ISearch

        public event EventHandler ResultItemSelected;
        public event EventHandler Cleared;

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

                    if (string.IsNullOrEmpty(value))
                    {
                        RaiseCleared();
                    }

                    IsResultsVisible = true;

                    OnPropertyChanged("Query");
                }
                if (UpdateResultsOnQueryChanges)
                {
                    MakeResults();
                }
            }
        }

        public bool UpdateResultsOnQueryChanges
        {
            get
            {
                return _resultsOnQueryChanges;
            }
            set
            {
                if (_resultsOnQueryChanges != value)
                {
                    _resultsOnQueryChanges = value;
                    OnPropertyChanged("UpdateResultsOnQueryChanges");
                }
            }
        }

        public ObservableCollection<T> Results { get; protected set; }

        public T SelectedItem
        {
            get
            {
                if (SelectedIndex > -1 && SelectedIndex < Results.Count)
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
                    OnPropertyChanged("SelectedIndex");
                    OnPropertyChanged("SelectedItem");
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
                    OnPropertyChanged("IsSearchActive");
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
                    OnPropertyChanged("IsSearchFocused");
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
                // set to true only if IsSearchFocused
                if (_isResultsVisible != value && (value == IsSearchFocused || IsSearchFocused))
                {
                    _isResultsVisible = value;

                    OnPropertyChanged("IsResultsVisible");
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
                    OnPropertyChanged("SwitchedOn");
                }
            }
        }

        public void Clear()
        {
            Query = "";
        }


        public void OnSelected(T item)
        {
            if (onSelected != null && item != null && SwitchedOn)
            {
                onSelected(item);
            }
            RaiseResultItemSelected();
        }

        public void RaiseResultItemSelected() // public for selecting by mouse
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
        private void RaiseCleared()
        {
            var h = Cleared;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
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

            OnPropertyChanged("Results");

            if (Results.Count > 0)
                SelectedIndex = 0;
        }

        public PopupSearch(ISimpleSearcher<T> searcher, bool switchedOn = true, Action<T> onSelected = null)
        {
            this.searcher = searcher;
            this.onSelected = onSelected;

            SwitchedOn = switchedOn;
            Clear(); // no results made here

            UpdateResultsOnQueryChanges = true;
        }
    }
}