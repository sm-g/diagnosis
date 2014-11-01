﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System;
using Diagnosis.Models;

namespace Diagnosis.ViewModels.Search
{
    public class NewFilterViewModel<T> : ViewModelBase, IFilter<T> where T : IDomainEntity
    {
        private readonly INewSearcher<T> searcher;
        Func<string, IEnumerable<T>> finder;
        private string _query;
        private bool _resultsOnQueryChanges;

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
            foreach (var item in Results.Except(res).ToList())
            {
                Results.Remove(item);
            }
            foreach (var item in res.Except(Results).ToList())
            {
                Results.Add(item);
            }

            OnFiltered();
        }

        public void Clear()
        {
            Query = "";
        }
        #endregion IFilter

        public NewFilterViewModel(INewSearcher<T> searcher)
        {
            this.searcher = searcher;
            Results = new ObservableCollection<T>();
            UpdateResultsOnQueryChanges = true;
        }
        public NewFilterViewModel(Func<string, IEnumerable<T>> finder)
        {
            this.finder = finder;
            Results = new ObservableCollection<T>();
            UpdateResultsOnQueryChanges = true;
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