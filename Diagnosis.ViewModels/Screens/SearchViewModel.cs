using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using Diagnosis.ViewModels.Autocomplete;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;
using Diagnosis.Common.Types;

namespace Diagnosis.ViewModels.Screens
{
    public class SearchViewModel : ToolViewModel
    {
        #region Fields

        private bool _all;
        private bool _controlsVisible;
        private HealthRecordQueryAndScope _scope;
        private HrSearchOptions _options;
        private IEnumerable<HrCategoryViewModel> _categories;

        private SearchResultViewModel _res;

        private EventMessageHandlersManager msgManager;

        #endregion Fields

        public const string ToolContentId = "Search";

        public SearchViewModel()
        {
            ContentId = ToolContentId;

            var rec = new Recognizer(Session) { AddNotPersistedToSuggestions = false, MeasureEditorWithCompare = true };
            AutocompleteAll = new AutocompleteViewModel(
                rec,
                AutocompleteViewModel.OptionsMode.Search,
                null);
            AutocompleteAny = new AutocompleteViewModel(
                rec,
                AutocompleteViewModel.OptionsMode.Search,
                null);
            AutocompleteNot = new AutocompleteViewModel(
                rec,
                AutocompleteViewModel.OptionsMode.Search,
                null);
            ControlsVisible = true;
            //   AllWords = true;

            AutocompleteAll.InputEnded += AutocompleteAll_InputEnded;
            AutocompleteAny.InputEnded += AutocompleteAll_InputEnded;
            AutocompleteNot.InputEnded += AutocompleteAll_InputEnded;
            AutocompleteAll.Tags.CollectionChanged += Tags_CollectionChanged;
            AutocompleteAny.Tags.CollectionChanged += Tags_CollectionChanged;
            AutocompleteNot.Tags.CollectionChanged += Tags_CollectionChanged;

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

        public IList<HrCategoryViewModel> SelectedCategories
        {
            get { return Categories.Where(cat => cat.IsChecked).ToList(); }
        }

        public bool AllEmpty
        {
            get
            {
                return SelectedCategories.Count() == 0
                    && AutocompleteAll.Tags.Count == 1
                    && AutocompleteAny.Tags.Count == 1
                    && AutocompleteNot.Tags.Count == 1;
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return new RelayCommand(Search, () => !AllEmpty);
            }
        }

        public AutocompleteViewModel AutocompleteAll { get; set; }

        public AutocompleteViewModel AutocompleteAny { get; set; }

        public AutocompleteViewModel AutocompleteNot { get; set; }

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

        /// <summary>
        /// Опции поиска при последнем поиске.
        /// </summary>
        public HrSearchOptions Options
        {
            get { return _options; }
            set
            {
                _options = value;
                OnPropertyChanged("Options");
            }
        }

        void AutocompleteAll_InputEnded(object sender, BoolEventArgs e)
        {
            if (SearchCommand.CanExecute(null))
                SearchCommand.Execute(null);
        }

        void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested(); // when drop tag, search button still disabled
            OnPropertyChanged("AllEmpty");
        }

        #region Options bindings

        public IEnumerable<HrCategoryViewModel> Categories
        {
            get
            {
                if (_categories == null)
                {
                    var cats = new List<HrCategory>(Session.QueryOver<HrCategory>().List());
                    cats.Add(HrCategory.Null);

                    _categories = cats
                        .OrderBy(cat => cat.Ord)
                        .Select(cat => new HrCategoryViewModel(cat))
                        .ToList();
                    _categories.ForAll(cat => cat.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "IsChecked")
                        {
                            OnPropertyChanged("SelectedCategories");
                            OnPropertyChanged("AllEmpty");
                        }
                    });
                }
                return _categories;
            }
        }

        public bool AllWords
        {
            get
            {
                return _all;
            }
            set
            {
                if (_all != value)
                {
                    _all = value;
                    OnPropertyChanged(() => AllWords);
                }
            }
        }

        public HealthRecordQueryAndScope QueryScope
        {
            get
            {
                return _scope;
            }
            set
            {
                if (_scope != value)
                {
                    _scope = value;
                    AllWords = true;
                    OnPropertyChanged(() => QueryScope);
                }
            }
        }

        #endregion Options bindings
        private HrSearchOptions MakeOptions()
        {
            var options = new HrSearchOptions();

            options.AllWords = AllWords;
            options.QueryScope = QueryScope;

            options.WordsAll = AutocompleteAll.GetCHIOs().Where(x => x.HIO is Word).Select(x => x.HIO).Cast<Word>().ToList();
            options.WordsAny = AutocompleteAny.GetCHIOs().Where(x => x.HIO is Word).Select(x => x.HIO).Cast<Word>().ToList();
            options.WordsNot = AutocompleteNot.GetCHIOs().Where(x => x.HIO is Word).Select(x => x.HIO).Cast<Word>().ToList();

            options.MeasuresAll = AutocompleteAll.GetCHIOs().Where(x => x.HIO is MeasureOp).Select(x => x.HIO).Cast<MeasureOp>().ToList();
            options.MeasuresAny = AutocompleteAny.GetCHIOs().Where(x => x.HIO is MeasureOp).Select(x => x.HIO).Cast<MeasureOp>().ToList();

            options.Categories = SelectedCategories.Select(cat => cat.category).ToList();

            Options = options;
            return options;
        }

        private void Search()
        {
            MakeOptions();
            var hrs = new HrSearcher().Search(Session, Options);
            Result = new SearchResultViewModel(hrs);
            ControlsVisible = false;
        }

        private void RecieveHealthRecords(IEnumerable<HealthRecord> hrs)
        {
            // все слова из записей
            var allWords = hrs.Aggregate(
                new HashSet<Word>(),
                (words, hr) =>
                {
                    hr.Words.ForAll((w) => words.Add(w));
                    return words;
                });

            // если несколько записей — любое из слов
            AllWords = hrs.Count() != 1;

            // все категории из записей
            Categories.ForAll((cat) => cat.IsChecked = false);
            var allCats = hrs.Aggregate(
                new HashSet<HrCategory>(),
                (cats, hr) =>
                {
                    cats.Add(hr.Category);
                    return cats;
                });
            Categories.Where(cat => allCats.Contains(cat.category)).ForAll(cat => cat.IsChecked = true);

            // давность из последней записи
            //var lastHr = hrs.Last();
            //HrDateOffsetLower = new DateOffset(lastHr.FromYear, lastHr.FromMonth, lastHr.FromDay);

            RecieveHrItemObjects(allWords);
        }

        private void RecieveHrItemObjects(IEnumerable<IHrItemObject> hios)
        {
            AutocompleteAll.ReplaceTagsWith(hios);

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

        public class HrCategoryViewModel : CheckableBase, IComparable
        {
            internal readonly HrCategory category;

            public HrCategoryViewModel(HrCategory category)
            {
                Contract.Requires(category != null);
                this.category = category;
            }

            public string Name
            {
                get
                {
                    return category.Title;
                }
            }
            public int CompareTo(object obj)
            {
                var other = obj as HrCategoryViewModel;
                if (other == null)
                    return -1;

                return this.category.CompareTo(other.category);
            }

            public override string ToString()
            {
                return category.ToString();
            }
        }
    }
}