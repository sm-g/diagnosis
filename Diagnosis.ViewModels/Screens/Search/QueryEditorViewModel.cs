using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using NHibernate;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Search
{
    public class QueryEditorViewModel : ViewModelBase
    {
        private ISession Session;
        private Action onQbEnter;
        private VisibleRelayCommand _send;

        public QueryEditorViewModel(ISession session, Action onQbEnter = null)
        {
            Contract.Requires(session != null);

            this.Session = session;
            this.onQbEnter = onQbEnter ?? (Action)(() => { });

            var loader = new JsonOptionsLoader(Session);
            Loader = new OptionsLoaderViewModel(this, loader);
            var hist = new History<SearchOptions>();
            History = new SearchHistoryViewModel(hist);
            QueryBlocks = new ObservableCollection<QueryBlockViewModel>();

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

            AuthorityController.LoggedIn += (s, e) =>
            {
                SetupQueryBlocks();
            };
            AuthorityController.LoggedOut += (s, e) =>
            {
                QueryBlocks.ForAll(x => x.Dispose());
                QueryBlocks.Clear();
            };

            SetupQueryBlocks();
        }

        public ObservableCollection<QueryBlockViewModel> QueryBlocks { get; private set; }

        public SearchHistoryViewModel History { get; set; }

        public OptionsLoaderViewModel Loader { get; set; }

        public bool AllEmpty { get { return QueryBlocks.All(x => x.AllEmpty); } }

        public VisibleRelayCommand SendToSearchCommand
        {
            get
            {
                return _send ?? (_send = new VisibleRelayCommand(() =>
                {
                    this.Send(Event.SendToSearch, GetOptions().AsParams(MessageKeys.ToSearchPackage));
                }, () => !AllEmpty)
                {
                    IsVisible = true
                });
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
        public void SetOptions(SearchOptions opt)
        {
            var root = QueryBlocks.FirstOrDefault();
            if (root != null && root.MakeOptions().Equals(opt))
                return;

            if (AuthorityController.CurrentDoctor == null)
                return;

            SetRootOptions(opt);
        }
        private void SetupQueryBlocks()
        {
            if (AuthorityController.CurrentDoctor != null)
                // делаем suggsetionmaker и автокомплиты к нему
                SetRootOptions();
        }

        private QueryBlockViewModel SetRootOptions(SearchOptions options = null)
        {
            Contract.Requires(AuthorityController.CurrentDoctor != null);
            Contract.Ensures(options == null || options.PartialLoaded == Contract.Result<QueryBlockViewModel>().Options.PartialLoaded);

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