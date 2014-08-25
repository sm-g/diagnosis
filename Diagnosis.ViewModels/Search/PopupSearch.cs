using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class PopupSearch<T> : ViewModelBase, IFilter<T> where T : ViewModelBase
    {
        #region Fields

        internal readonly ISimpleSearcher<T> searcher;
        private readonly Action<T> onSelected;

        private string _query;
        private int _selectedIndex = -1;
        private ICommand _clearCommand;
        private ICommand _searchCommand;
        private ICommand _resultsCommand;
        private ICommand _selectCommand;
        private bool _searchActive;
        private bool _searchFocused;
        private bool _isResultsVisible;
        private bool _resultsOnQueryChanges;
        private bool _switchedOn;

        #endregion Fields

        public event VmBaseEventHandler ResultItemSelected;

        public event EventHandler Cleared;

        #region IFilter

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
                        OnCleared();
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

        public ObservableCollection<T> Results { get; protected set; }

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
        public bool IsQueryEmpty
        {
            get { return string.IsNullOrWhiteSpace(Query); }
        }

        public ICommand ClearCommand
        {
            get
            {
                return _clearCommand ?? (_clearCommand = new RelayCommand(Clear,
                    () => !IsQueryEmpty && SwitchedOn));
            }
        }
        public void Clear()
        {
            Query = "";
        }

        #endregion IFilter

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
                return _selectCommand ?? (_selectCommand = new RelayCommand(
                    () => RaiseResultItemSelected(SelectedItem),
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

        /// <summary>
        /// Для выбора элемента, который не совпадает с SelectedItem (SearchTree).
        /// </summary>
        public void OnSelected(object item)
        {
            T asT = item as T;
            if (asT == null)
            {
                throw new ArgumentException("Selected item type is wrong.");
            }

            if (onSelected != null && SwitchedOn)
            {
                onSelected(asT);
            }
            RaiseResultItemSelected(asT);
        }

        public void RaiseResultItemSelected(T item) // public for selecting by mouse in FloatSearch
        {
            if (SwitchedOn)
            {
                var h = ResultItemSelected;
                if (h != null)
                {
                    h(this, new VmBaseEventArgs(item));
                }
            }
            IsResultsVisible = false;
        }

        private void OnCleared()
        {
            var h = Cleared;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
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