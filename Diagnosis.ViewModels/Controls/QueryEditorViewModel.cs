﻿using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using NHibernate;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Controls
{
    public class QueryEditorViewModel : ViewModelBase
    {
        private ISession session;
        private Action onQbEnter;
        private VisibleRelayCommand _send;
        private EventAggregator.EventMessageHandler handler;

        public QueryEditorViewModel(ISession session, Action onQbEnter = null)
        {
            Contract.Requires(session != null);

            this.session = session;
            this.onQbEnter = onQbEnter ?? (Action)(() => { });

            var loader = new JsonOptionsLoader();
            Loader = new OptionsLoaderViewModel(this, loader, session);
            var hist = new History<SearchOptions>();
            History = new HistoryViewModel<SearchOptions>(hist);
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

            handler = this.Subscribe(Event.NewSession, (e) =>
            {
                var s = e.GetValue<ISession>(MessageKeys.Session);
                ReplaceSession(s);
            });
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

        public HistoryViewModel<SearchOptions> History { get; set; }

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

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    handler.Dispose();
                    QueryBlocks.Clear();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void ReplaceSession(ISession s)
        {
            if (this.session.SessionFactory == s.SessionFactory)
                this.session = s;
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

            var qb = new QueryBlockViewModel(session, onQbEnter, options);
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
    }
}