using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Data.Search;
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
        public const string ToolContentId = "Search";

        private bool _controlsVisible;
        private AbstractSearchResultViewModel _res;
        private bool _mode;
        private int _searchTabIndex;
        private ObservableCollection<Estimator> _estimators;
        private Estimator _est;
        private OptionsLoader loader;
        private EventMessageHandlersManager msgManager;

        public SearchViewModel()
        {
            ContentId = ToolContentId;

            loader = new JsonOptionsLoader(Session);

            QueryEditor = new QueryEditorViewModel(Session, () =>
            {
                if (SearchCommand.CanExecute(null))
                    SearchCommand.Execute(null);
            });
            QueryEditor.SendToSearchCommand.IsVisible = false;

            var ests = EntityQuery<Estimator>.All(Session)();
            Estimators = new ObservableCollection<Estimator>(ests);
            SelectedEstimator = ests.FirstOrDefault();

            msgManager = new EventMessageHandlersManager(
                this.Subscribe(Event.SendToSearch, (e) =>
                {
                    object pack = null;
                    try
                    {
                        pack = e.GetValue<object>(MessageKeys.ToSearchPackage);
                    }
                    catch
                    {
                        return;
                    }

                    var hrs = pack as IEnumerable<HealthRecord>;
                    var hios = pack as IEnumerable<ConfWithHio>;
                    var opt = pack as SearchOptions;

                    if (hrs != null && hrs.Any())
                        RecieveHealthRecords(hrs);
                    else if (hios != null && hios.Any())
                        RecieveHrItemObjects(hios);
                    else if (opt != null)
                        RecieveOptions(opt);

                    Contract.Assume(LastRecieverQueryBlock != null);
                    Activate();
                })

            );

            ControlsVisible = true;
        }

        public ICommand SearchCommand
        {
            get
            {
                return new RelayCommand(Search,
                    () => AuthorityController.CurrentDoctor != null &&
                        ((IsCriteriaSearch && SelectedEstimator != null) // выбран критерий
                        || !QueryEditor.AllEmpty)); // есть условия поиска
            }
        }

        public QueryEditorViewModel QueryEditor { get; private set; }

        public AbstractSearchResultViewModel Result
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
                return Result != null && (Result as dynamic).Statistic.PatientsCount == 0;
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

        public int SearchTabIndex
        {
            get
            {
                return _searchTabIndex;
            }
            set
            {
                if (_searchTabIndex != value)
                {
                    _searchTabIndex = value;
                    OnPropertyChanged(() => SearchTabIndex);
                    OnPropertyChanged(() => IsCriteriaSearch);
                }
            }
        }

        public ObservableCollection<Estimator> Estimators
        {
            get
            {
                return _estimators;
            }
            set
            {
                if (_estimators != value)
                {
                    _estimators = value;
                    OnPropertyChanged(() => Estimators);
                }
            }
        }

        public Estimator SelectedEstimator
        {
            get
            {
                return _est;
            }
            set
            {
                if (_est != value)
                {
                    _est = value;
                    OnPropertyChanged(() => SelectedEstimator);
                }
            }
        }
        public RelayCommand EditEstimatorCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.EditCrit, SelectedEstimator.AsParams(MessageKeys.Crit));
                }, () => SelectedEstimator != null);
            }
        }
        internal QueryBlockViewModel RootQueryBlock { get { return QueryEditor.QueryBlocks.FirstOrDefault(); } }

        /// <summary>
        /// Блок, принявший отправленное в поиск.
        /// </summary>
        internal QueryBlockViewModel LastRecieverQueryBlock { get; private set; }

        private bool IsCriteriaSearch { get { return SearchTabIndex == 1; } }

        private void Search()
        {
            RemoveLastResults();

            if (IsCriteriaSearch)
            {
                Estimator est = SelectedEstimator;

                var crOpts = est.CriteriaGroups
                    .SelectMany(x => x.Criteria)
                    .Select(x => new { Cr = x, Opt = loader.ReadOptions(x.Options) })
                    .Where(x => x.Opt != null);

                var crHrs = crOpts.ToDictionary(x => x.Cr, x => Searcher.GetResult(Session, x.Opt));
                var hOpt = loader.ReadOptions(est.Options);
                var topHrs = Searcher.GetResult(Session, hOpt);
                Result = new CritSearchResultViewModel(crHrs, topHrs, est);
            }
            else
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

                Result = new HrsSearchResultViewModel(shrs, options);
                QueryEditor.History.Memo(options);
            }
#if !DEBUG
            ControlsVisible = false;
#endif
        }

        private void RecieveHealthRecords(IEnumerable<HealthRecord> hrs)
        {
            var qb = GetRecieverQb();

            // все слова из записей (кроме слов измерений)
            var allCWords = hrs.Aggregate(
                new HashSet<Confindencable<Word>>(),
                (words, hr) =>
                {
                    hr.GetCWordsNotFromMeasure().ForAll(w => words.Add(w));
                    return words;
                });
            // все измерения с оператором
            var allMops = hrs.Aggregate(
                new HashSet<Confindencable<MeasureOp>>(),
                (mops, hr) =>
                {
                    hr.HrItems
                        .Where(x => x.Measure != null)
                        .Select(x => x.Measure.ToMeasureOp().AsConfidencable(x.Confidence))
                        .ForAll(x => mops.Add(x));
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
            // измерения с оператором
            chios = chios.Select(x => x.HIO is Measure ? (x.HIO as Measure).ToMeasureOp().AsConfWithHio(x.Confidence) : x);
            qb.AutocompleteAll.ReplaceTagsWith(chios);

            RemoveLastResults();
        }

        private void RecieveOptions(SearchOptions opt)
        {
            QueryEditor.SetOptions(opt);
            LastRecieverQueryBlock = RootQueryBlock;

            Search();
            ControlsVisible = false; // не надо поправлять запрос в поиске, редактируем критерии
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
            if (Result != null)
                Result.Dispose();
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