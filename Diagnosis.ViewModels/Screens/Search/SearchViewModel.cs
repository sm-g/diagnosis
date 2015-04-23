using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Diagnosis.ViewModels.Screens
{
    public class SearchViewModel : ToolViewModel
    {
        private bool _controlsVisible;
        private SearchResultViewModel _res;
        private bool _mode;

        private EventMessageHandlersManager msgManager;

        public const string ToolContentId = "Search";
        private SearchOptions _options;

        public SearchViewModel()
        {
            ContentId = ToolContentId;

            ControlsVisible = true;
            History = new SearchHistory();
            History.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CurrentOptions")
                {
                    LoadOptions(History.CurrentOptions);
                }
            };
            Loader = new OptionsLoader(Session, this);

            QueryBlocks = new ObservableCollection<QueryBlockViewModel>();
            AddRootQb();
            QueryBlocks.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(() => AllEmpty);
            };

#if DEBUG
            //QueryBlocks[0].AddChildQbCommand.Execute(null);
            //QueryBlocks[0].AddChildQbCommand.Execute(null);
            //QueryBlocks[0].AddChildQbCommand.Execute(null);
            //QueryBlocks[0].SearchScope = SearchScope.Holder;
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

        public SearchHistory History { get; set; }

        public OptionsLoader Loader { get; set; }

        private QueryBlockViewModel AddRootQb(SearchOptions opttions = null)
        {
            if (RootQueryBlock != null)
            {
                return RootQueryBlock;
            }
            var qb = new QueryBlockViewModel(Session,
                () =>
                {
                    if (SearchCommand.CanExecute(null))
                        SearchCommand.Execute(null);
                },
                opttions);
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
            var options = QueryBlocks[0].MakeOptions();
            if (UseOldMode)
            {
                shrs = new HrSearcher().SearchOld(Session, RootQueryBlock.GetOldOptions());
            }
            else
            {
                shrs = HrSearcher.GetResult(Session, options);
            }

            Result = new SearchResultViewModel(shrs, options);
            History.AddOptions(options);
#if !DEBUG            
            ControlsVisible = false;
#endif
        }

        public void LoadOptions(SearchOptions opt)
        {
            QueryBlocks.Clear();
            AddRootQb(opt);
            History.AddOptions(opt);
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