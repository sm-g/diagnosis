using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            ControlsVisible = true;

            QueryBlocks = new ObservableCollection<QueryBlockViewModel>();
            AddRootQb();
            QueryBlocks.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(() => AllEmpty);
            };

#if DEBUG
            QueryBlocks[0].AddChildQbCommand.Execute(null);
            QueryBlocks[0].AddChildQbCommand.Execute(null);
            QueryBlocks[0].AddChildQbCommand.Execute(null);
            QueryBlocks[0].SearchScope = SearchScope.Holder;
#endif

            msgManager = new EventMessageHandlersManager(
                this.Subscribe(Event.SendToSearch, (e) =>
                {
                    IEnumerable<HealthRecord> hrs = null;
                    IEnumerable<IHrItemObject> hios = null;
                    try
                    {
                        hrs = e.GetValue<IEnumerable<HealthRecord>>(MessageKeys.HealthRecords);
                    }
                    catch { }
                    if (hrs != null && hrs.Count() > 0)
                    {
                        RecieveHealthRecords(hrs);
                    }
                    else
                    {
                        try
                        {
                            hios = e.GetValue<IEnumerable<IHrItemObject>>(MessageKeys.HrItemObjects);
                        }
                        catch { }
                        if (hios != null && hios.Count() > 0)
                        {
                            RecieveHrItemObjects(hios);
                        }
                    }

                    Activate();
                })

            );

#if !DEBUG
            UseOldMode = true;
#endif
        }

        public bool AllEmpty
        {
            get
            {
                return QueryBlocks.All(x => x.AllEmpty);
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return new RelayCommand(Search, () => !AllEmpty);
            }
        }

        public ObservableCollection<QueryBlockViewModel> QueryBlocks { get; set; }

        public QueryBlockViewModel RootQueryBlock { get { return QueryBlocks.FirstOrDefault(); } }

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
                    if (value && QueryBlocks.Count == 0)
                        AddRootQb();
                    OnPropertyChanged(() => UseOldMode);
                }
            }
        }

        private QueryBlockViewModel AddRootQb()
        {
            if (RootQueryBlock != null)
            {
                return RootQueryBlock;
            }
            var qb = new QueryBlockViewModel(Session, () =>
                {
                    if (SearchCommand.CanExecute(null))
                        SearchCommand.Execute(null);
                });
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

        private void Search()
        {
            IEnumerable<HealthRecord> shrs;
            if (UseOldMode)
            {
                QueryBlocks[0].MakeOptions();
                shrs = new HrSearcher().SearchOld(Session, QueryBlocks[0].Options);
            }
            else
            {
                shrs = GetResult(QueryBlocks[0]);
            }

            Result = new SearchResultViewModel(shrs);
            ControlsVisible = false;
        }

        private IEnumerable<HealthRecord> GetResult(QueryBlockViewModel qb)
        {
            if (qb.IsGroup)
            {
                var qbResults = (from q in qb.Children
                                 select new
                                 {
                                     Qb = q,
                                     Hrs = GetResult(q)
                                 }).ToDictionary(x => x.Qb, x => x.Hrs);
                switch (qb.SearchScope)
                {
                    case SearchScope.HealthRecord:
                        return InOneHr(qbResults);

                    case SearchScope.Holder:
                        if (qb.All)
                            return InOneHolder(qbResults);
                        else
                            return AnyInOneHolder(qbResults);

                    case SearchScope.Patient:
                        if (qb.All)
                            return InOneScope(qbResults);
                        else
                            return AnyInOneHolder(qbResults);

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                var opt = qb.MakeOptions();
                return new HrSearcher().Search(Session, opt);
            }
        }

        /// <summary>
        /// Все в том же списке.
        ///
        /// В каждом блоке должны найтись записи из списка, тогда список попадает в результаты,
        /// где есть все найденные записи из этих списков.
        /// </summary>
        /// <param name="results"></param>
        /// <returns>Все подходящие записи из подходящих списков</returns>
        private static IEnumerable<HealthRecord> InOneHolder(IDictionary<QueryBlockViewModel, IEnumerable<HealthRecord>> results)
        {
            // плоско все найденные Блок-Список-Записи
            var qbHolderHrs = from qbHrs in results
                              let middle =
                                  from hr in qbHrs.Value
                                  group hr by hr.Holder into g
                                  select new { Hrs = g.Cast<HealthRecord>(), Holder = g.Key }
                              from q in middle
                              select new { Qb = qbHrs.Key, Hrs = q.Hrs, Holder = q.Holder };

            var qbCount = results.Keys.Count;

            // подходящие списки и подходящие записи из них
            var holderHrs = from a in qbHolderHrs
                            group a by a.Holder into g
                            where g.Count() == qbCount // список во всех блоках
                            select new
                            {
                                Holder = g.Key,
                                Hrs = from b in qbHolderHrs
                                      where b.Holder == g.Key
                                      from hr in b.Hrs
                                      select hr
                            };

            // записи из них среди найденных
            var hrs = from a in holderHrs
                      from hr in a.Hrs
                      select hr;

            return hrs.Distinct();
        }

        /// <summary>
        /// Все в одной записи
        ///
        /// В результатах пересечение записей каждого блока.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static IEnumerable<HealthRecord> InOneHr(Dictionary<QueryBlockViewModel, IEnumerable<HealthRecord>> results)
        {
            Contract.Requires(results.Count > 0);

            IEnumerable<HealthRecord> shrs = results[results.Keys.First()];
            for (int i = 1; i < results.Count - 1; i++)
            {
                shrs = shrs.Intersect(results[results.Keys.ElementAt(i)]);
            }
            return shrs;
        }

        /// <summary>
        /// Все в одной области
        ///
        /// В каждом блоке должны найтись записи из пациента, тогда пациент попадает в результаты,
        /// где есть все найденные записи из него.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static IEnumerable<HealthRecord> InOneScope(Dictionary<QueryBlockViewModel, IEnumerable<HealthRecord>> results)
        {
            // плоско все найденные Блок-Список-Записи
            var qbPatientHrs = from qbHrs in results
                               let middle =
                                   from hr in qbHrs.Value
                                   group hr by hr.GetPatient() into g
                                   select new { Hrs = g.Cast<HealthRecord>(), Patient = g.Key }
                               from q in middle
                               select new { Qb = qbHrs.Key, Hrs = q.Hrs, Patient = q.Patient };

            var qbCount = results.Keys.Count;

            // подходящие списки и подходящие записи из них
            var holderHrs = from a in qbPatientHrs
                            group a by a.Patient into g
                            where g.Count() == qbCount // пациент во всех блоках
                            select new
                            {
                                Patient = g.Key,
                                Hrs = from b in qbPatientHrs
                                      where b.Patient == g.Key
                                      from hr in b.Hrs
                                      select hr
                            };

            // записи из них среди найденных
            var hrs = from a in holderHrs
                      from hr in a.Hrs
                      select hr;

            return hrs.Distinct();
        }

        /// <summary>
        /// Любое в том же списке
        ///
        /// Список попадет в результаты, если нашлась хотя бы одна запись из списка в любом блоке.
        /// То есть все записи проходят в результаты.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static IEnumerable<HealthRecord> AnyInOneHolder(IDictionary<QueryBlockViewModel, IEnumerable<HealthRecord>> results)
        {
            return from hrs in results.Values
                   from hr in hrs
                   select hr;

            // список и записи из него
            var q = from hrs in results.Values
                    from hr in hrs
                    group hr by hr.Holder into g
                    select new { Holder = g.Key, Hrs = g };

            return from groupedByHolder in q
                   from hr in groupedByHolder.Hrs
                   select hr;
        }

        private void RecieveHealthRecords(IEnumerable<HealthRecord> hrs)
        {
            var qb = QueryBlocks.FirstOrDefault();
            if (!UseOldMode)
            {
                RootQueryBlock.AddChildQbCommand.Execute(null);
                qb = RootQueryBlock.Children.Last();
            }

            // все слова из записей
            var allWords = hrs.Aggregate(
                new HashSet<Word>(),
                (words, hr) =>
                {
                    hr.Words.ForAll((w) => words.Add(w));
                    return words;
                });

            // если несколько записей — любое из слов
            qb.AllWords = hrs.Count() != 1;

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

            qb.AutocompleteAll.ReplaceTagsWith(allWords);
            RemoveLastResults();
        }

        private void RecieveHrItemObjects(IEnumerable<IHrItemObject> hios)
        {
            var qb = QueryBlocks.FirstOrDefault();
            if (!UseOldMode)
            {
                RootQueryBlock.AddChildQbCommand.Execute(null);
                qb = RootQueryBlock.Children.Last();
            }
            qb.AutocompleteAll.ReplaceTagsWith(hios);

            RemoveLastResults();
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
                QueryBlocks.Clear();
            }
            base.Dispose(disposing);
        }
    }
}