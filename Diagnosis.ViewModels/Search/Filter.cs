using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System;

namespace Diagnosis.ViewModels
{
    public class FilterViewModel<T> : ViewModelBase, IFilter<T>
    {
        private ISimpleSearcher<T> _searcher;
        private string _query;
        private bool _resultsOnQueryChanges;
        public ISimpleSearcher<T> Searcher
        {
            get { return _searcher; }
            set
            {
                if (_searcher != value && value != null)
                {
                    _searcher = value;
                }
            }
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
            if (IsQueryEmpty)
            {
                res = Searcher.Search("");
            }
            else
            {
                res = Searcher.Search(Query);
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

        public FilterViewModel(ISimpleSearcher<T> searcher)
        {
            Searcher = searcher;
            Results = new ObservableCollection<T>();
            UpdateResultsOnQueryChanges = true;
        }

        private void OnCleared()
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