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
            AddNewQb();
            QueryBlocks.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(() => AllEmpty);

            };

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
                        AddNewQb();
                    OnPropertyChanged(() => UseOldMode);
                }
            }
        }

        public RelayCommand AddQbCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    AddNewQb();
                });
            }
        }

        private QueryBlockViewModel AddNewQb()
        {
            var qb = new QueryBlockViewModel();
            qb.Search = this;
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
                var results = new Dictionary<QueryBlockViewModel, IEnumerable<HealthRecord>>();
                foreach (var qb in QueryBlocks)
                {
                    // ищем записи по каждому блоку
                    qb.MakeOptions();
                    var hrs = new HrSearcher().Search(Session, qb.Options);
                    results.Add(qb, hrs);
                }


                shrs = InOneHolder(results);
            }

            Result = new SearchResultViewModel(shrs);
            ControlsVisible = false;
        }
        /// <summary>
        /// все в том же списке
        /// 
        /// в каждом блоке должны найтись записи из списка, тогда список попадет в результаты,
        /// где надо объединить все записи по спискам
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static IEnumerable<HealthRecord> InOneHolder(Dictionary<QueryBlockViewModel, IEnumerable<HealthRecord>> results)
        {
            IEnumerable<HealthRecord> shrs;
            var grs = new Dictionary<QueryBlockViewModel, IEnumerable<IGrouping<IHrsHolder, HealthRecord>>>();
            foreach (var qb in results.Keys)
            {
                var gr = results[qb].GroupBy(x => x.Holder);
                grs.Add(qb, gr);
            }

            var allHolders = grs.Values.SelectMany(x => x.Select(y => y.Key)).Distinct();

            var q = (from h in allHolders
                     where grs.Keys.All(x => grs[x].Select(y => y.Key).Contains(h))
                     select new
                     {
                         h,
                         hrs = grs.Values
                             .SelectMany(x => x
                             .Where(y => y.Key == h)
                             .SelectMany(t => t.ToList()))
                             .ToList()
                     }
            ).ToDictionary(x => x.h, x => x.hrs.Distinct());
            shrs = q.Values.SelectMany(x => x);
            return shrs;
        }
        /// <summary>
        /// все в одной записи
        /// 
        /// в результатах пересечение записей каждого блока
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
        /// все в одной области
        /// 
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static IEnumerable<HealthRecord> InOneScope(Dictionary<QueryBlockViewModel, IEnumerable<HealthRecord>> results)
        {
            throw new NotImplementedException();
        }

        private void RecieveHealthRecords(IEnumerable<HealthRecord> hrs)
        {
            var options = QueryBlocks.FirstOrDefault();
            if (!UseOldMode || options == null)
            {
                options = AddNewQb();
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
            options.AllWords = hrs.Count() != 1;

            // все категории из записей
            options.Categories.ForAll((cat) => cat.IsChecked = false);
            var allCats = hrs.Aggregate(
                new HashSet<HrCategory>(),
                (cats, hr) =>
                {
                    cats.Add(hr.Category);
                    return cats;
                });
            options.Categories.Where(cat => allCats.Contains(cat.category)).ForAll(cat => cat.IsChecked = true);

            // давность из последней записи
            //var lastHr = hrs.Last();
            //HrDateOffsetLower = new DateOffset(lastHr.FromYear, lastHr.FromMonth, lastHr.FromDay);

            options.AutocompleteAll.ReplaceTagsWith(allWords);
            RemoveLastResults();
        }

        private void RecieveHrItemObjects(IEnumerable<IHrItemObject> hios)
        {
            var options = QueryBlocks.FirstOrDefault();
            if (!UseOldMode || options == null)
            {
                options = AddNewQb();
            }
            options.AutocompleteAll.ReplaceTagsWith(hios);

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
            }
            base.Dispose(disposing);
        }
    }
}