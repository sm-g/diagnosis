using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels.DataTransfer;
using Diagnosis.ViewModels.Screens;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Search
{
    public class QueryEditorViewModel : ViewModelBase
    {
        private ISession Session;
        private Action onQbEnter;
        public QueryEditorViewModel() { }
        public QueryEditorViewModel(ISession session, Action onQbEnter)
        {
            this.Session = session;
            this.onQbEnter = onQbEnter;

            var loader = new JsonOptionsLoader(Session);
            var hist = new History<SearchOptions>();
            Loader = new OptionsLoaderViewModel(this, loader);
            History = new SearchHistoryViewModel(hist);
            QueryBlocks = new ObservableCollection<QueryBlockViewModel>();
            SetRootOptions();


            QueryBlocks.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(() => AllEmpty);
            };
            hist.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CurrentState")
                {
                    SetOptions(hist.CurrentState);
                }
            };


        }
        public ObservableCollection<QueryBlockViewModel> QueryBlocks { get; private set; }

        public SearchHistoryViewModel History { get; set; }

        public OptionsLoaderViewModel Loader { get; set; }

        public bool AllEmpty
        {
            get
            {
                return QueryBlocks.All(x => x.AllEmpty);
            }
        }

        public SearchOptions GetOptions()
        {
            var options = QueryBlocks[0].GetSearchOptions();
            return options;
        }
        /// <summary>
        /// Меняет опции. Текущие опции теряются, если не совпадают.
        /// </summary>
        /// <param name="opt"></param>
        public void SetOptions(SearchOptions opt)
        {
            var root = QueryBlocks.FirstOrDefault();
            if (root != null && opt.Equals(root.Options))
                return;

            SetRootOptions(opt);
        }

        private QueryBlockViewModel SetRootOptions(SearchOptions options = null)
        {
            QueryBlocks.Clear();

            var qb = new QueryBlockViewModel(Session, onQbEnter, options);
            qb.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "AllEmpty")
                {
                    OnPropertyChanged(() => AllEmpty);
                }
            };
            QueryBlocks.Add(qb);
            qb.IsExpanded = true;
            return qb;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    QueryBlocks.Clear();
                }

            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
