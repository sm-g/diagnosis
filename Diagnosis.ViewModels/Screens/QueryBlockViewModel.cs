using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Search;
using EventAggregator;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class QueryBlockViewModel : HierarchicalBase<QueryBlockViewModel>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(QueryBlockViewModel));
        private ISession session;
        private Action executeSearch;

        private bool _allWords;
        private HealthRecordQueryAndScope _scope;
        private HrSearchOptions _options;
        private IList<HrCategoryViewModel> _categories;
        private SearchScope _sscope;
        private bool _all;
        private bool _group;
        private VisibleRelayCommand _removeQbCommand;
        private VisibleRelayCommand _addSyblingQbCommand;

        public QueryBlockViewModel(ISession session, Action executeSearch)
        {
            this.session = session;
            this.executeSearch = executeSearch;

            var rec = new Recognizer(session) { AddNotPersistedToSuggestions = false, MeasureEditorWithCompare = true };
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

            AutocompleteAll.InputEnded += Autocomplete_InputEnded;
            AutocompleteAny.InputEnded += Autocomplete_InputEnded;
            AutocompleteNot.InputEnded += Autocomplete_InputEnded;
            AutocompleteAll.PropertyChanged += Autocomplete_PropertyChanged;
            AutocompleteAny.PropertyChanged += Autocomplete_PropertyChanged;
            AutocompleteNot.PropertyChanged += Autocomplete_PropertyChanged;
            AutocompleteAll.Tags.CollectionChanged += Tags_CollectionChanged;
            AutocompleteAny.Tags.CollectionChanged += Tags_CollectionChanged;
            AutocompleteNot.Tags.CollectionChanged += Tags_CollectionChanged;

            Children.CollectionChanged += (s, e) =>
            {
                IsGroup = Children.Count > 0;
                OnPropertyChanged(() => AllEmpty);

                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        Options.Children.Add((e.NewItems[0] as QueryBlockViewModel).Options);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        Options.Children.Remove((e.OldItems[0] as QueryBlockViewModel).Options);
                        break;
                    default:
                        break;
                }
            };

            All = true;
            AnyMin = 1;

            this.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case "IsSelected":
                    case "IsGroup":
                    case "IsExpanded":
                        RefreshDescription();
                        OnPropertyChanged(() => DescriptionVisible);
                        break;
                }
            };
        }
        [Obsolete("For xaml only.")]
        public QueryBlockViewModel()
        {
        }

        public AutocompleteViewModel AutocompleteAll { get; set; }

        public AutocompleteViewModel AutocompleteAny { get; set; }

        public AutocompleteViewModel AutocompleteNot { get; set; }

        public bool AllEmpty
        {
            get
            {
                // если групповой блок - только дети, не учитывается содержимое,
                // которое оставлено для удобства после удаления детей
                return (IsGroup && Children.All(x => x.AllEmpty))
                    ||
                    (!IsGroup && SelectedCategories.Count() == 0
                    && AutocompleteAll.IsEmpty
                    && AutocompleteAny.IsEmpty
                    && AutocompleteNot.IsEmpty);
            }
        }

        private int _anyMin;
        public int AnyMin
        {
            get
            {
                return _anyMin;
            }
            set
            {
                if (_anyMin != value)
                {
                    _anyMin = value;
                    OnPropertyChanged(() => AnyMin);
                }
            }
        }

        public IEnumerable<int> AnyMinValues { get { return Enumerable.Range(1, 3); } }

        /// <summary>
        /// Опции поиска.
        /// </summary>
        public HrSearchOptions Options
        {
            get { return _options ?? (_options = MakeOptions()); }
            private set
            {
                _options = value;
                logger.DebugFormat("{0}", value);
                OnPropertyChanged("Options");
            }
        }

        public IList<HrCategoryViewModel> Categories
        {
            get
            {
                if (_categories == null && session != null)
                {
                    var cats = new List<HrCategory>(session.QueryOver<HrCategory>().List());
                    cats.Add(HrCategory.Null);

                    _categories = cats
                        .OrderBy(cat => cat.Ord)
                        .Select(cat => new HrCategoryViewModel(cat))
                        .ToList();
                    _categories.ForAll(cat => cat.CheckedChanged += (s, e) =>
                    {
                        OnPropertyChanged(() => SelectedCategories);
                        OnPropertyChanged(() => AllEmpty);
                        RefreshDescription();
                    });
                }
                return _categories;
            }
        }
        public IList<HrCategoryViewModel> SelectedCategories
        {
            get { return Categories.Where(cat => cat.IsChecked).ToList(); }
        }

        /// <summary>
        /// Групповой блок, с детьми.
        /// </summary>
        public bool IsGroup
        {
            get
            {
                return _group;
            }
            private set
            {
                if (_group != value)
                {
                    _group = value;
                    OnPropertyChanged(() => IsGroup);
                    OnPropertyChanged(() => DescriptionVisible);
                }
            }
        }

        /// <summary>
        /// Исключающий блок, только "без".
        /// </summary>
        public bool IsExcluding
        {
            get
            {
                return !IsGroup && AutocompleteAll.IsEmpty &&
                                   AutocompleteAny.IsEmpty &&
                                  !AutocompleteNot.IsEmpty;
            }
        }

        public bool DescriptionVisible
        {
            get
            {
                return (!IsSelected && !IsGroup) ||
                        !IsExpanded && IsGroup;
            }
        }

        #region Old

        public bool AllWords
        {
            get
            {
                return _allWords;
            }
            set
            {
                if (_allWords != value)
                {
                    _allWords = value;
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

        #endregion Old

        public SearchScope SearchScope
        {
            get
            {
                return _sscope;
            }
            set
            {
                if (_sscope != value)
                {
                    _sscope = value;
                    RefreshDescription();
                    OnPropertyChanged(() => SearchScope);
                }
            }
        }
        /// <summary>
        /// Применить все блоки к области поиска (или достаточно одного).
        /// </summary>
        public bool All
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
                    RefreshDescription();
                    OnPropertyChanged(() => All);
                }
            }
        }

        public VisibleRelayCommand RemoveQbCommand
        {
            get
            {
                return _removeQbCommand ?? (_removeQbCommand = new VisibleRelayCommand(() =>
                {
                    this.Remove();
                    this.PropertyChanged -= qb_PropertyChanged;
                }, () => !IsRoot)
                {
                    IsVisible = !IsRoot
                });
            }
        }

        public RelayCommand AddChildQbCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    AddChildQb();
                });
            }
        }

        public VisibleRelayCommand AddSyblingQbCommand
        {
            get
            {
                return _addSyblingQbCommand ?? (_addSyblingQbCommand = new VisibleRelayCommand(() =>
                {
                    Parent.AddChildQb();
                }, () => !IsRoot)
                {
                    IsVisible = !IsRoot
                });
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return new RelayCommand(executeSearch);
            }
        }

        public HrSearchOptions MakeOptions()
        {
            var options = new HrSearchOptions();

            options.WordsAll = AutocompleteAll.GetCHIOs().Where(x => x.HIO is Word).Select(x => x.HIO).Cast<Word>().ToList();
            options.WordsAny = AutocompleteAny.GetCHIOs().Where(x => x.HIO is Word).Select(x => x.HIO).Cast<Word>().ToList();
            options.WordsNot = AutocompleteNot.GetCHIOs().Where(x => x.HIO is Word).Select(x => x.HIO).Cast<Word>().ToList();

            options.MeasuresAll = AutocompleteAll.GetCHIOs().Where(x => x.HIO is MeasureOp).Select(x => x.HIO).Cast<MeasureOp>().ToList();
            options.MeasuresAny = AutocompleteAny.GetCHIOs().Where(x => x.HIO is MeasureOp).Select(x => x.HIO).Cast<MeasureOp>().ToList();

            options.Categories = SelectedCategories.Select(cat => cat.category).ToList();
            options.MinAny = AnyMin;
            options.All = All;
            options.Scope = SearchScope;

            if (_options != null) // копируем детей
            {
                _options.Children.ForAll(x =>
                    options.Children.Add(x));
            }
            if (!IsRoot && _options != null) // изменились - обновляем ссылку в родительских опциях через родительский блок
            {
                Parent.Options.Children.Remove(_options);
                Parent.Options.Children.Add(options);
            }
            Options = options;
            return options;
        }
        public OldHrSearchOptions GetOldOptions()
        {
            var options = new OldHrSearchOptions();

            options.AllWords = AllWords;
            options.QueryScope = QueryScope;

            options.WordsAll = AutocompleteAll.GetCHIOs().Where(x => x.HIO is Word).Select(x => x.HIO).Cast<Word>().ToList();

            options.MeasuresAll = AutocompleteAll.GetCHIOs().Where(x => x.HIO is MeasureOp).Select(x => x.HIO).Cast<MeasureOp>().ToList();

            options.Categories = SelectedCategories.Select(cat => cat.category).ToList();
            return options;
        }

        internal void SelectCategory(params HrCategory[] cats)
        {
            var vms = from c in cats
                      join vm in Categories on c equals vm.category
                      select vm;
            vms.ForEach(x => x.IsChecked = true);

        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AutocompleteAll.InputEnded -= Autocomplete_InputEnded;
                AutocompleteAny.InputEnded -= Autocomplete_InputEnded;
                AutocompleteNot.InputEnded -= Autocomplete_InputEnded;
                AutocompleteAll.Tags.CollectionChanged -= Tags_CollectionChanged;
                AutocompleteAny.Tags.CollectionChanged -= Tags_CollectionChanged;
                AutocompleteNot.Tags.CollectionChanged -= Tags_CollectionChanged;
                AutocompleteAll.PropertyChanged -= Autocomplete_PropertyChanged;
                AutocompleteAny.PropertyChanged -= Autocomplete_PropertyChanged;
                AutocompleteNot.PropertyChanged -= Autocomplete_PropertyChanged;
                AutocompleteAll.Dispose();
                AutocompleteAny.Dispose();
                AutocompleteNot.Dispose();
            }
            base.Dispose(disposing);
        }
        public override string ToString()
        {
            return string.Format("{0} {1}", All, SearchScope);
        }

        private void RefreshDescription()
        {
            if (DescriptionVisible)
                MakeOptions();
        }

        private QueryBlockViewModel AddChildQb()
        {
            var qb = new QueryBlockViewModel(session, executeSearch);

            qb.PropertyChanged += qb_PropertyChanged;

            Add(qb);
            this.IsExpanded = true;
            return qb;
        }

        private void qb_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AllEmpty")
            {
                OnPropertyChanged(() => AllEmpty);
            }
        }
        private void Autocomplete_InputEnded(object sender, BoolEventArgs e)
        {
            executeSearch();
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested(); // when drop tag, search button still disabled

        }

        private void Autocomplete_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsEmpty")
            {
                OnPropertyChanged(() => AllEmpty);
                RefreshDescription();
            }
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            // листовой блок - все в записи
            //Contract.Invariant(IsGroup || SearchScope == SearchScope.HealthRecord && All);
        }
    }
}