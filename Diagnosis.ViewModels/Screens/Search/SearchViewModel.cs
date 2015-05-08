﻿using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class SearchViewModel : ToolViewModel
    {
        private bool _controlsVisible;
        private SearchResultViewModel _res;
        private bool _mode;
        private EventMessageHandlersManager msgManager;

        public const string ToolContentId = "Search";

        public SearchViewModel()
        {
            ContentId = ToolContentId;

            var hist = new History<SearchOptions>();

            QueryEditor = new QueryEditorViewModel(Session, () =>
            {
                if (SearchCommand.CanExecute(null))
                    SearchCommand.Execute(null);
            });

            var loader = new JsonOptionsLoader(Session);

            msgManager = new EventMessageHandlersManager(
                this.Subscribe(Event.SendToSearch, (e) =>
                {
                    IEnumerable<HealthRecord> hrs = null;
                    try
                    {
                        hrs = e.GetValue<IEnumerable<HealthRecord>>(MessageKeys.HealthRecords);
                    }
                    catch { }
                    if (hrs != null && hrs.Any())
                    {
                        RecieveHealthRecords(hrs);
                    }
                    else
                    {
                        IEnumerable<ConfWithHio> hios = null;
                        try
                        {
                            hios = e.GetValue<IEnumerable<ConfWithHio>>(MessageKeys.Chios);
                        }
                        catch { }
                        if (hios != null && hios.Any())
                        {
                            RecieveHrItemObjects(hios);
                        }
                    }

                    Activate();
                })

            );

            ControlsVisible = true;
        }

        public ICommand SearchCommand
        {
            get
            {
                return new RelayCommand(Search, () => !QueryEditor.AllEmpty);
            }
        }

        public QueryEditorViewModel QueryEditor { get; private set; }


        public SearchResultViewModel Result
        {
            get
            {
                return _res;
            }
            set
            {
                if (_res != value)
                {
                    _res = value;
                    OnPropertyChanged(() => Result);
                    OnPropertyChanged(() => NothingFound);
                }
            }
        }

        /// <summary>
        /// Показывать сообщение «ничего не найдено»
        /// </summary>
        public bool NothingFound
        {
            get
            {
                return Result != null && Result.Statistic.PatientsCount == 0;
            }
        }

        /// <summary>
        /// Показывать поисковую форму
        /// </summary>
        public bool ControlsVisible
        {
            get
            {
                return _controlsVisible;
            }
            set
            {
                if (_controlsVisible != value)
                {
                    _controlsVisible = value;
                    OnPropertyChanged("ControlsVisible");
                }
            }
        }

        public bool UseOldMode
        {
            get
            {
                return _mode;
            }
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    OnPropertyChanged(() => UseOldMode);
                }
            }
        }
        internal QueryBlockViewModel RootQueryBlock { get { return QueryEditor.QueryBlocks.FirstOrDefault(); } }
        /// <summary>
        /// Блок, принявший отправленное в поиск.
        /// </summary>
        internal QueryBlockViewModel LastRecieverQueryBlock { get; private set; }

        private void Search()
        {
            IEnumerable<HealthRecord> shrs;
            var options = QueryEditor.GetOptions();
            if (UseOldMode)
            {
                shrs = HrSearcher.SearchOld(Session, RootQueryBlock.GetOldOptions());
            }
            else
            {
                shrs = Searcher.GetResult(Session, options);
            }

            Result = new SearchResultViewModel(shrs, options);
            //hist.Memorize(options);
#if !DEBUG
            ControlsVisible = false;
#endif
        }

        private void RecieveHealthRecords(IEnumerable<HealthRecord> hrs)
        {
            var qb = GetRecieverQb();

            // все слова из записей
            var allCWords = hrs.Aggregate(
                new HashSet<Confindencable<Word>>(),
                (words, hr) =>
                {
                    hr.GetCWords().ForAll(w => words.Add(w));
                    return words;
                });
            // все измерения с оператором
            var allMops = hrs.Aggregate(
                new HashSet<MeasureOp>(),
                (mops, hr) =>
                {
                    hr.Measures.ForAll(m => mops.Add(m.ToMeasureOp()));
                    return mops;
                });

            if (UseOldMode)
            {
                // если несколько записей — любое из слов
                qb.AllWords = hrs.Count() != 1;
            }

            // все категории из записей
            qb.Categories.ForAll((cat) => cat.IsChecked = false);
            var allCats = hrs.Aggregate(
                new HashSet<HrCategory>(),
                (cats, hr) =>
                {
                    cats.Add(hr.Category);
                    return cats;
                });
            qb.Categories.Where(cat => allCats.Contains(cat.category)).ForAll(cat => cat.IsChecked = true);

            // давность из последней записи
            //var lastHr = hrs.Last();
            //HrDateOffsetLower = new DateOffset(lastHr.FromYear, lastHr.FromMonth, lastHr.FromDay);

            qb.AutocompleteAll.ReplaceTagsWith(allCWords.Union<object>(allMops));
            RemoveLastResults();
        }
        private void RecieveHrItemObjects(IEnumerable<ConfWithHio> chios)
        {
            var qb = GetRecieverQb();
            qb.AutocompleteAll.ReplaceTagsWith(chios);

            RemoveLastResults();
        }

        private QueryBlockViewModel GetRecieverQb()
        {
            Contract.Ensures(Contract.Result<QueryBlockViewModel>() != null);
            QueryBlockViewModel qb;

            if (UseOldMode)
            {
                qb = QueryEditor.QueryBlocks.FirstOrDefault();
            }
            else
            {
                RootQueryBlock.AddChildQbCommand.Execute(null);
                qb = RootQueryBlock.Children.Last();
            }
            LastRecieverQueryBlock = qb;
            return qb;
        }

        /// <summary>
        /// Убирает результаты предыдущего поиска
        /// </summary>
        private void RemoveLastResults()
        {
            Result = null;
            ControlsVisible = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                msgManager.Dispose();
                QueryEditor.Dispose();
                if (Result != null)
                    Result.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}