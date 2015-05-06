using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Search;
using EventAggregator;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private SearchOptions _options;
        private IList<HrCategoryViewModel> _categories;
        private SearchScope _sscope;
        private int _anyMin;
        private bool _group;
        private VisibleRelayCommand _removeQbCommand;
        private VisibleRelayCommand _addSyblingQbCommand;

        private QueryGroupOperator _operator;

        public QueryBlockViewModel(ISession session, Action executeSearch, SearchOptions options = null)
        {
            this.session = session;
            this.executeSearch = executeSearch;

            _operator = QueryGroupOperator.All;
            _anyMin = 1;

            CreateAutocompletes(session);

            Children.CollectionChanged += (s, e) =>
            {
                IsGroup = Children.Count > 0;
                OnPropertyChanged(() => AllEmpty);

                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        var qb = (e.NewItems[0] as QueryBlockViewModel);
                        Options.Children.Add(qb.Options);
                        break;

                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        qb = (e.OldItems[0] as QueryBlockViewModel);
                        Options.Children.Remove(qb.Options);
                        RefreshDescription();
                        break;

                    default:
                        break;
                }
            };

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

            AnyMinMenuItems = new ObservableCollection<MenuItem>()
            {
                new MenuItem("один", new RelayCommand(()=>AnyMin = 1)),
                new MenuItem("два", new RelayCommand(()=>AnyMin = 2)),
                new MenuItem("три", new RelayCommand(()=>AnyMin = 3)),
            };
            GroupOperatorMenuItems = new ObservableCollection<MenuItem>()
            {
                new MenuItem("всё", new RelayCommand(()=>GroupOperator=QueryGroupOperator.All)),
                new MenuItem("любое", new RelayCommand(()=>GroupOperator=QueryGroupOperator.Any)),
            };
            SearchScopeMenuItems = new ObservableCollection<MenuItem>()
            {
                new MenuItem("запись", new RelayCommand(()=>SearchScope = Models.SearchScope.HealthRecord)),
                new MenuItem("список", new RelayCommand(()=>SearchScope = Models.SearchScope.Holder)),
                new MenuItem("пациент", new RelayCommand(()=>SearchScope = Models.SearchScope.Patient)),
            };

            if (options != null)
            {
                FillFromOptions(options);
                Options.PartialLoaded = options.PartialLoaded; // TODO не делять опции снова
            }
        }

        [Obsolete("For xaml only.")]
        public QueryBlockViewModel()
        {
        }

        public ObservableCollection<MenuItem> AnyMinMenuItems { get; private set; }
        public ObservableCollection<MenuItem> GroupOperatorMenuItems { get; private set; }
        public ObservableCollection<MenuItem> SearchScopeMenuItems { get; private set; }

        public IQbAutocompleteViewModel AutocompleteAll { get; set; }

        public IQbAutocompleteViewModel AutocompleteAny { get; set; }

        public IQbAutocompleteViewModel AutocompleteNot { get; set; }

        public bool AllEmpty
        {
            get
            {
                // если групповой блок - только дети, не учитывается содержимое,
                // которое оставлено для удобства после удаления детей
                return (IsGroup && Children.All(x => x.AllEmpty))
                    ||
                    (!IsGroup && !SelectedCategories.Any()
                    && AutocompleteAll.IsEmpty
                    && AutocompleteAny.IsEmpty
                    && AutocompleteNot.IsEmpty);
            }
        }

        public bool AnyPopupOpen
        {
            get
            {
                return AutocompleteAll.IsPopupOpen ||
                    AutocompleteAny.IsPopupOpen ||
                    AutocompleteNot.IsPopupOpen;
            }
        }

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
                    RefreshDescription();
                    OnPropertyChanged(() => AnyMin);
                }
            }
        }

        /// <summary>
        /// Опции поиска.
        /// </summary>
        public SearchOptions Options
        {
            get { return _options ?? (_options = MakeOptions()); }
            private set
            {
                _options = value;
                logger.DebugFormat("options set: {0} \n{1}", this, value);
                OnPropertyChanged("Options");
            }
        }

        public IList<HrCategoryViewModel> Categories
        {
            get
            {
                if (_categories == null && session != null)
                {
                    var cats = new List<HrCategory>() { HrCategory.Null };

                    try
                    {
                        cats.AddRange(session.QueryOver<HrCategory>().List());
                    }
                    catch
                    {
                    }

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

        /// <summary>
        /// Надо обновить опции для поиска.
        /// Если опсиание видно, опции всегда свежие.
        /// </summary>
        public bool NeedRefresh { get { return !DescriptionVisible; } }

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
        /// Применить блоки к области поиска и выбрать записи, которые удовлетворяют всем/любому/не любому.
        /// </summary>
        public QueryGroupOperator GroupOperator
        {
            get
            {
                return _operator;
            }
            set
            {
                if (_operator != value)
                {
                    _operator = value;
                    RefreshDescription();
                    OnPropertyChanged(() => GroupOperator);
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
                    this.Dispose();
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
                    var brother = Parent.AddChildQb();
                    brother.IsSelected = true;
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


        public SearchOptions MakeOptions()
        {
            var options = new SearchOptions(IsRoot);

            options.WordsAll = AutocompleteAll.GetCHIOs().Where(x => x.HIO is Word).Select(x => x.HIO).Cast<Word>().ToList();
            options.WordsAny = AutocompleteAny.GetCHIOs().Where(x => x.HIO is Word).Select(x => x.HIO).Cast<Word>().ToList();
            options.WordsNot = AutocompleteNot.GetCHIOs().Where(x => x.HIO is Word).Select(x => x.HIO).Cast<Word>().ToList();

            options.MeasuresAll = AutocompleteAll.GetCHIOs().Where(x => x.HIO is MeasureOp).Select(x => x.HIO).Cast<MeasureOp>().ToList();
            options.MeasuresAny = AutocompleteAny.GetCHIOs().Where(x => x.HIO is MeasureOp).Select(x => x.HIO).Cast<MeasureOp>().ToList();

            options.Categories = SelectedCategories.Select(cat => cat.category).ToList();
            options.MinAny = AnyMin;
            options.GroupOperator = GroupOperator;
            options.SearchScope = SearchScope;

            if (_options != null) // копируем детей
            {
                _options.Children.ForAll(x =>
                    options.Children.Add(x));
            }
            if (!IsRoot && _options != null)
            {
                // изменились
                // обновляем ссылку в родительских опциях через родительский блок
                // у родителей не видно опсиание, поэтому не надо менять опции, при поиске все равно обновляем
                Contract.Assume(!Parent.DescriptionVisible);
                Parent.Options.Children.Remove(_options);
                Parent.Options.Children.Add(options);
            }

            Options = options;
            return options;
        }
        /// <summary>
        /// Свежие опции, пригодные для поиска
        /// </summary>
        /// <returns></returns>
        public SearchOptions GetSearchOptions()
        {
            if (NeedRefresh)
            {
                return MakeOptions();
            }
            Contract.Assume(Options.DeepClone().Equals(MakeOptions()));
            return Options;
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

        public override string ToString()
        {
            return string.Format("{0}: {1},{2}", Level, GroupOperator, SearchScope);
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
                AutocompleteAll.CHiosChanged -= Autocomplete_CHiosChanged;
                AutocompleteAny.CHiosChanged -= Autocomplete_CHiosChanged;
                AutocompleteNot.CHiosChanged -= Autocomplete_CHiosChanged;
                AutocompleteAll.Tags.CollectionChanged -= Tags_CollectionChanged;
                AutocompleteAny.Tags.CollectionChanged -= Tags_CollectionChanged;
                AutocompleteNot.Tags.CollectionChanged -= Tags_CollectionChanged;
                AutocompleteAll.PropertyChanged -= Autocomplete_PropertyChanged;
                AutocompleteAny.PropertyChanged -= Autocomplete_PropertyChanged;
                AutocompleteNot.PropertyChanged -= Autocomplete_PropertyChanged;
                AutocompleteAll.Dispose();
                AutocompleteAny.Dispose();
                AutocompleteNot.Dispose();
                if (_categories != null)
                {
                    _categories.Clear();
                }
            }
            base.Dispose(disposing);
        }

        private void CreateAutocompletes(ISession session)
        {
            var rec = new SuggestionsMaker(session) { AddNotPersistedToSuggestions = false };
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
            AutocompleteAll.CHiosChanged += Autocomplete_CHiosChanged;
            AutocompleteAny.CHiosChanged += Autocomplete_CHiosChanged;
            AutocompleteNot.CHiosChanged += Autocomplete_CHiosChanged;
            AutocompleteAll.PropertyChanged += Autocomplete_PropertyChanged;
            AutocompleteAny.PropertyChanged += Autocomplete_PropertyChanged;
            AutocompleteNot.PropertyChanged += Autocomplete_PropertyChanged;
            AutocompleteAll.Tags.CollectionChanged += Tags_CollectionChanged;
            AutocompleteAny.Tags.CollectionChanged += Tags_CollectionChanged;
            AutocompleteNot.Tags.CollectionChanged += Tags_CollectionChanged;
        }



        private void FillFromOptions(SearchOptions options)
        {
            GroupOperator = options.GroupOperator;
            SearchScope = options.SearchScope;
            AnyMin = options.MinAny;
            SelectCategory(options.Categories.ToArray());

            AutocompleteAll.ReplaceTagsWith(options.WordsAll.Union<IHrItemObject>(options.MeasuresAll));
            AutocompleteAny.ReplaceTagsWith(options.WordsAny.Union<IHrItemObject>(options.MeasuresAny));
            AutocompleteNot.ReplaceTagsWith(options.WordsNot);

            foreach (var opt in options.Children)
                AddChildQb(opt);
        }
        /// <summary>
        /// Обновляем описание блока при каждом изменении запроса, если оно видно.
        /// Изменения: AnyMin, QueryScope, Сущности в автокомплитах, SelectedCats, GroupOperator, удаление ребенка, изменения у детей.
        /// Хотя при изменениях у детей описания не видно.
        /// </summary>
        private void RefreshDescription()
        {
            if (DescriptionVisible)
                MakeOptions();
        }

        private QueryBlockViewModel AddChildQb(SearchOptions options = null)
        {
            var qb = new QueryBlockViewModel(session, executeSearch, options);

            qb.PropertyChanged += qb_PropertyChanged;

            Add(qb);
            this.IsExpanded = true;
            return qb;
        }
        void Autocomplete_CHiosChanged(object sender, EventArgs e)
        {
            RefreshDescription();
        }

        private void qb_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AllEmpty")
            {
                OnPropertyChanged(() => AllEmpty);
            }
            else if (e.PropertyName == "Options")
            {
                RefreshDescription();
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
            switch (e.PropertyName)
            {
                case "IsEmpty":

                    OnPropertyChanged(() => AllEmpty);
                    RefreshDescription();
                    break;
                case "IsPopupOpen":
                    OnPropertyChanged(() => AnyPopupOpen);
                    break;
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