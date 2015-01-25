﻿using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Search
{
    public class FilterViewModel<T> : ViewModelBase, IFilter<T> where T : class
    {
        private readonly ISimpleSearcher<T> searcher;
        private Func<string, IEnumerable<T>> finder;
        private string _query;
        private bool _isFocused;
        private bool _resultsOnQueryChanges;

        public FilterViewModel(ISimpleSearcher<T> searcher)
        {
            this.searcher = searcher;
            Results = new ObservableCollection<T>();
            UpdateResultsOnQueryChanges = true;
        }

        public FilterViewModel(Func<string, IEnumerable<T>> finder)
        {
            this.finder = finder;
            Results = new ObservableCollection<T>();
            UpdateResultsOnQueryChanges = true;
        }

        #region IFilter

        public event EventHandler Cleared;

        public event EventHandler Filtered;

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
                    if (IsQueryEmpty)
                    {
                        OnCleared();
                    }
                    OnPropertyChanged("Query");
                    OnPropertyChanged("IsQueryEmpty");
                }
                if (UpdateResultsOnQueryChanges)
                {
                    Filter();
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

        public ICommand FilterCommand
        {
            get
            {
                return new RelayCommand(Filter);
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return new RelayCommand(Clear,
                    () => !IsQueryEmpty);
            }
        }

        public bool IsQueryEmpty
        {
            get { return string.IsNullOrWhiteSpace(Query); }
        }

        public void Filter()
        {
            IEnumerable<T> res;
            if (searcher != null)
                if (IsQueryEmpty)
                {
                    res = searcher.Search("");
                }
                else
                {
                    res = searcher.Search(Query);
                }
            else
                if (IsQueryEmpty)
                {
                    res = finder.Invoke("");
                }
                else
                {
                    res = finder.Invoke(Query);
                }

            Results.SyncWith(res);

            OnFiltered();
        }

        public void Clear()
        {
            Query = "";
        }

        #endregion IFilter

        public bool IsFocused
        {
            get
            {
                return _isFocused;
            }
            set
            {
                if (_isFocused != value)
                {
                    _isFocused = value;
                    OnPropertyChanged("IsFocused");
                }
            }
        }
        protected void OnCleared()
        {
            var h = Cleared;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        protected virtual void OnFiltered()
        {
            var h = Filtered;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }
    }
}