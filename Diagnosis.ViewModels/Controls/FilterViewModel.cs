using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Controls
{
    public class FilterViewModel<T> : ViewModelBase, IFilter<T>
    {
        private readonly Func<string, IEnumerable<T>> finder;
        private string _query;
        private bool _isFocused;
        private int _updResBound;
        private bool _resultsOnQueryChanges;
        private bool _autoFiltered;

        public FilterViewModel(Func<string, IEnumerable<T>> finder)
        {
            this.finder = finder;
            Results = new ObservableCollection<T>();
            DoAutoFilter = true;
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
                if (DoAutoFilter && _query != null && _query.Length >= AutoFilterMinQueryLength)
                {
                    AutoFiltered = true;
                    Filter();
                }
                else
                {
                    AutoFiltered = false;
                }
            }
        }

        public ObservableCollection<T> Results { get; protected set; }

        public bool DoAutoFilter
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
                    OnPropertyChanged(() => DoAutoFilter);
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

        public int AutoFilterMinQueryLength
        {
            get { return _updResBound; }
            set { _updResBound = value; }
        }

        public bool AutoFiltered
        {
            get
            {
                return _autoFiltered;
            }
            set
            {
                if (_autoFiltered != value)
                {
                    _autoFiltered = value;
                    OnPropertyChanged(() => AutoFiltered);
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