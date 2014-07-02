using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class FilterViewModel<T> : ViewModelBase, IFilter<T>
    {
        internal readonly ISimpleSearcher<T> searcher;

        private string _query;
        private bool _resultsOnQueryChanges;
        private ICommand _clearCommand;
        private ICommand _filterCommand;

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

        public ICommand ClearCommand
        {
            get
            {
                return _clearCommand ?? (_clearCommand = new RelayCommand(Clear,
                    () => !IsQueryEmpty));
            }
        }

        public bool IsQueryEmpty
        {
            get { return string.IsNullOrWhiteSpace(Query); }
        }

        public void Clear()
        {
            Query = "";
        }

        #endregion IFilter

        public ICommand FilterCommand
        {
            get
            {
                return _filterCommand
                   ?? (_filterCommand = new RelayCommand(MakeResults));
            }
        }

        private void MakeResults()
        {
            IEnumerable<T> res;
            if (IsQueryEmpty)
            {
                res = searcher.Search("");
            }
            else
            {
                res = searcher.Search(Query);
            }
            Results.Clear();
            foreach (var item in res)
            {
                Results.Add(item);
            }
        }

        public FilterViewModel(ISimpleSearcher<T> searcher)
        {
            this.searcher = searcher;
            Results = new ObservableCollection<T>();
            UpdateResultsOnQueryChanges = true;
        }
    }
}