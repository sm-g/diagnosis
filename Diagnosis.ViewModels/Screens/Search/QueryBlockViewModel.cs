using Diagnosis.Common;
using Diagnosis.Common.Util;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
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
        private Action onAutocompleteInputEnded;

        private bool _allWords;
        private HealthRecordQueryAndScope _scope;
        private SearchOptions _options;
        private IList<HrCategoryViewModel> _categories;
        private SearchScope _sscope;
        private int _minAny;
        private bool _group;
        private VisibleRelayCommand _removeQbCommand;
        private VisibleRelayCommand _addSyblingQbCommand;
        private QueryGroupOperator _operator;
        private bool inFilling;
        private bool _withConf;
        private ReentrantFlag inMakingOptions = new ReentrantFlag();
        private EventMessageHandler handler;

        /// <summary>
        ///
        /// </summary>
        /// <param name="session">Для автокомплитов и категорий</param>
        /// <param name="onAutocompleteInputEnded"></param>
        /// <param name="options"></param>
        public QueryBlockViewModel(ISession session, Action onAutocompleteInputEnded, SearchOptions options = null)
        {
            Contract.Ensures(options == null || options.Equals(_options));

            this.session = session;
            this.onAutocompleteInputEnded = onAutocompleteInputEnded;

            _operator = QueryGroupOperator.All;
            _minAny = 1;

            CreateAutocompletes(session);
            CreateMenuItems();

            Children.CollectionChanged += Children_CollectionChanged;

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

                    case "Options":
                        if (!IsRoot)
                            Parent.RefreshDescription();
                        break;
                }
            };
            handler = this.Subscribe(Event.NewSession, (e) =>
            {
                var s = e.GetValue<ISession>(MessageKeys.Session);
                if (this.session.SessionFactory == s.SessionFactory)
                    this.session = s;

            });

            if (options != null)
            {
                _options = options;
                FillFromOptions(options);
            }
        }

        [Obsolete("For xaml only.")]
        public QueryBlockViewModel()
        {
        }

        public ObservableCollection<MenuItem> MinAnyMenuItems { get; private set; }

        public ObservableCollection<MenuItem> GroupOperatorMenuItems { get; private set; }

        public ObservableCollection<MenuItem> SearchScopeMenuItems { get; private set; }

        public string MinAnyString { get { return MinAnyMenuItems[MinAny - 1].Text; } }

        #region Options bindings

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

        public IQbAutocompleteViewModel AutocompleteAll { get; set; }

        public IQbAutocompleteViewModel AutocompleteAny { get; set; }

        public IQbAutocompleteViewModel AutocompleteNot { get; set; }

        public IList<HrCategoryViewModel> Categories
        {
            get
            {
                if (_categories == null && session != null)
                {
                    _categories = EntityQuery<HrCategory>.All(session)()
                        .Union(HrCategory.Null)
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

        public int MinAny
        {
            get
            {
                return _minAny;
            }
            set
            {
                if (_minAny != value)
                {
                    _minAny = value;
                    RefreshDescription();
                    OnPropertyChanged(() => MinAny);
                    OnPropertyChanged(() => MinAnyString);
                }
            }
        }

        public bool WithConfidence
        {
            get
            {
                return _withConf;
            }
            set
            {
                if (_withConf != value)
                {
                    _withConf = value;
                    RefreshDescription();
                    OnPropertyChanged(() => WithConfidence);
                }
            }
        }

        #endregion Options bindings

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

        /// <summary>
        /// Опции поиска.
        /// </summary>
        public SearchOptions Options
        {
            get { return _options ?? (_options = MakeOptions()); }
            private set
            {
                if (inFilling) return;

                _options = value;
                logger.DebugFormat("options set: {0} \n{1}", this, value);
                OnPropertyChanged("Options");
            }
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
                    if (!_group)
                    {
                        // invariant
                        _sscope = SearchScope.HealthRecord;
                        _operator = QueryGroupOperator.All;
                        OnPropertyChanged(() => SearchScope);
                        OnPropertyChanged(() => GroupOperator);
                    }
                    OnPropertyChanged(() => IsGroup);
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
                       (!IsExpanded && IsGroup);
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
                    SearchOptions opt = null;
                    if (Children.Count == 0)
                    {
                        // copy info when making group
                        opt = new SearchOptions();
                        FillToOptions(opt);
                    }

                    AddChildQb(opt);
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

        public SearchOptions MakeOptions()
        {
            if (inMakingOptions.CanEnter)
                using (inMakingOptions.Enter())
                {
                    var options = new SearchOptions(IsRoot);

                    FillToOptions(options);

                    if (_options != null)
                    {
                        // обновляем детские опции
                        Children.ForAll(qb =>
                        {
                            qb.Options = qb.MakeOptions();
                            options.Children.Add(qb.Options);
                        });
                    }
                    if (!IsRoot && _options != null)
                    {
                        // изменились опции
                        // обновляем ссылку в родительских опциях через родительский блок

                        Parent.Options.Children.Remove(_options);
                        Parent.Options.Children.Add(options);
                    }

                    return options;
                }

            Contract.Assume(_options != null); // можем вернуть готовые опции
            return Options;
        }

        /// <summary>
        /// Свежие опции, пригодные для поиска
        /// </summary>
        public SearchOptions GetSearchOptions()
        {
            Contract.Requires(IsRoot);

            Options = MakeOptions();
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
                handler.Dispose();

                Children.CollectionChanged -= Children_CollectionChanged;

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
            var sm = new SuggestionsMaker(session) { AddNotPersistedToSuggestions = false };
            AutocompleteAll = new QueryBlockAutocomplete(sm);
            AutocompleteAny = new QueryBlockAutocomplete(sm);
            AutocompleteNot = new QueryBlockAutocomplete(sm, new[] { BlankType.Word }); // без measure

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

        private void CreateMenuItems()
        {
            MinAnyMenuItems = new ObservableCollection<MenuItem>()
            {
                new MenuItem("один", new RelayCommand(()=>MinAny = 1)),
                new MenuItem("два", new RelayCommand(()=> MinAny = 2)),
                new MenuItem("три", new RelayCommand(()=> MinAny = 3)),
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
        }

        private void FillFromOptions(SearchOptions options)
        {
            inFilling = true;
            // TODO automap
            GroupOperator = options.GroupOperator;
            SearchScope = options.SearchScope;
            MinAny = options.MinAny;
            WithConfidence = options.WithConf;
            SelectCategory(options.Categories.ToArray());

            AutocompleteAll.ReplaceTagsWith(options.CWordsAll.Union<object>(options.MeasuresAll));
            AutocompleteAny.ReplaceTagsWith(options.CWordsAny.Union<object>(options.MeasuresAny));
            AutocompleteNot.ReplaceTagsWith(options.CWordsNot);

            foreach (var opt in options.Children)
                AddChildQb(opt);
            inFilling = false;
        }

        private void FillToOptions(SearchOptions options)
        {
            AutocompleteAll.CompleteTypings();
            AutocompleteAny.CompleteTypings();
            AutocompleteNot.CompleteTypings();

            options.CWordsAll = AutocompleteAll.GetCWords().ToList();
            options.CWordsAny = AutocompleteAny.GetCWords().ToList();
            options.CWordsNot = AutocompleteNot.GetCWords().ToList();

            options.MeasuresAll = AutocompleteAll.GetCHIOs().Where(x => x.HIO is MeasureOp).Select(x => x.HIO).Cast<MeasureOp>().ToList();
            options.MeasuresAny = AutocompleteAny.GetCHIOs().Where(x => x.HIO is MeasureOp).Select(x => x.HIO).Cast<MeasureOp>().ToList();

            options.Categories = SelectedCategories.Select(cat => cat.category).ToList();
            options.MinAny = MinAny;
            options.WithConf = WithConfidence;
            options.GroupOperator = GroupOperator;
            options.SearchScope = SearchScope;
        }

        /// <summary>
        /// Обновляем описание блока при каждом изменении запроса, если оно видно.
        /// Изменения: MinAny, SearchScope, WithConf, Сущности в автокомплитах, SelectedCats, GroupOperator, удаление ребенка, изменения у детей.
        /// Хотя при изменениях у детей описания не видно.
        /// </summary>
        private void RefreshDescription()
        {
            if (DescriptionVisible && inMakingOptions.CanEnter)
                Options = MakeOptions();
        }

        private QueryBlockViewModel AddChildQb(SearchOptions options = null)
        {
            var qb = new QueryBlockViewModel(session, onAutocompleteInputEnded, options);

            qb.PropertyChanged += qb_PropertyChanged;

            this.IsExpanded = true; // теперь описание скрыто
            Add(qb);
            return qb;
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsGroup = Children.Count > 0;
            OnPropertyChanged(() => AllEmpty);

            if (inFilling) return;

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
        }

        private void Autocomplete_CHiosChanged(object sender, EventArgs e)
        {
            RefreshDescription();
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
            onAutocompleteInputEnded();
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
            Contract.Invariant(IsGroup || SearchScope == SearchScope.HealthRecord && GroupOperator == QueryGroupOperator.All);
        }
    }
}