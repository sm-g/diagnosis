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
        private bool _searchActive;
        private bool _searchFocused;
        private bool _isResultsVisible;
        private bool _resultsOnQueryChanges;

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
                return new RelayCommand(Clear,
                    () => !IsQueryEmpty);
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
                return new RelayCommand(() =>
                        {
                            IsSearchActive = !IsSearchActive;
                        }
                       );
            }
        }

        public ICommand ToggleResultsVisibleCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            IsResultsVisible = !IsResultsVisible;
                        }
                       );
            }
        }

        public ICommand SelectCommand
        {
            get
            {
                return new RelayCommand(
                    () => RaiseResultItemSelected(SelectedItem));
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

            if (onSelected != null)
            {
                onSelected(asT);
            }
            RaiseResultItemSelected(asT);
        }

        public void RaiseResultItemSelected(T item) // public for selecting by mouse in FloatSearch
        {
                var h = ResultItemSelected;
                if (h != null)
                {
                    h(this, new VmBaseEventArgs(item));
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

        public PopupSearch(ISimpleSearcher<T> searcher, Action<T> onSelected = null)
        {
            this.searcher = searcher;
            this.onSelected = onSelected;

            Clear(); // no results made here

            UpdateResultsOnQueryChanges = true;
        }
    }
}