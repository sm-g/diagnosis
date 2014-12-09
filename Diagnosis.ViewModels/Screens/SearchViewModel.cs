﻿using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using Diagnosis.ViewModels.Search.Autocomplete;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class SearchViewModel : SessionVMBase
    {
        #region Fields

        private HrSearchOptions _options;

        private bool _all;
        private int _allSCope;
        private int? _appDayLower;
        private int? _appDayUpper;
        private int? _appMonthLower;
        private int? _appMonthUpper;
        private int? _appYearLower;
        private int? _appYearUpper;
        private DateOffset _hrDateOffsetLower;
        private DateOffset _hrDateOffsetUpper;
        private bool _controlsVisible;

        private IEnumerable<HrCategoryViewModel> _categories;
        private SearchResultViewModel _res;
        private HrSearcher searcher = new HrSearcher();

        private EventMessageHandlersManager msgManager;

        #endregion Fields

        public SearchViewModel()
        {
            Autocomplete = new Autocomplete(new Recognizer(Session) { OnlyWords = true }, false, false, null);

            ControlsVisible = true;
            AllWords = true;

            Autocomplete.InputEnded += (s, e) =>
            {
                Search();
            };
            Autocomplete.Tags.CollectionChanged += (s, e) =>
            {
                CommandManager.InvalidateRequerySuggested(); // when drop tag, search button still disabled
                OnPropertyChanged("AllEmpty");
            };
            msgManager = new EventMessageHandlersManager(
                this.Subscribe(Events.SendToSearch, (e) =>
                {
                    try
                    {
                        var hrs = e.GetValue<IEnumerable<HealthRecord>>(MessageKeys.HealthRecords);
                        if (hrs != null && hrs.Count() > 0)
                        {
                            RecieveHealthRecords(hrs);
                        }
                    }
                    catch { }
                    try
                    {
                        var hios = e.GetValue<IEnumerable<IHrItemObject>>(MessageKeys.HrItemObjects);
                        if (hios != null && hios.Count() > 0)
                        {
                            RecieveHrItemObjects(hios);
                        }
                    }
                    catch { }
                })
            );
        }

        #region Options bindings

        public int? AppDayLower
        {
            get
            {
                return _appDayLower;
            }
            set
            {
                if (_appDayLower != value)
                {
                    _appDayLower = value;

                    OnPropertyChanged("AppDayLower");
                    OnPropertyChanged("AllEmpty");
                }
            }
        }

        public int? AppDayUpper
        {
            get
            {
                return _appDayUpper;
            }
            set
            {
                if (_appDayUpper != value)
                {
                    _appDayUpper = value;

                    OnPropertyChanged("AppDayUpper");
                    OnPropertyChanged("AllEmpty");
                }
            }
        }

        public int? AppMonthLower
        {
            get
            {
                return _appMonthLower;
            }
            set
            {
                if (_appMonthLower != value)
                {
                    _appMonthLower = value;

                    OnPropertyChanged("AppMonthLower");
                    OnPropertyChanged("AllEmpty");
                }
            }
        }

        public int? AppMonthUpper
        {
            get
            {
                return _appMonthUpper;
            }
            set
            {
                if (_appMonthUpper != value)
                {
                    _appMonthUpper = value;

                    OnPropertyChanged("AppMonthUpper");
                    OnPropertyChanged("AllEmpty");
                }
            }
        }

        public int? AppYearLower
        {
            get
            {
                return _appYearLower;
            }
            set
            {
                if (_appYearLower != value)
                {
                    _appYearLower = value;

                    OnPropertyChanged("AppYearLower");
                    OnPropertyChanged("AllEmpty");
                }
            }
        }

        public int? AppYearUpper
        {
            get
            {
                return _appYearUpper;
            }
            set
            {
                if (_appYearUpper != value)
                {
                    _appYearUpper = value;

                    OnPropertyChanged("AppYearUpper");
                    OnPropertyChanged("AllEmpty");
                }
            }
        }

        public IEnumerable<HrCategoryViewModel> Categories
        {
            get
            {
                if (_categories == null)
                {
                    var catsVM = Session.QueryOver<HrCategory>().List().Select(cat => new HrCategoryViewModel(cat)).ToList();
                    catsVM.ForAll(cat => cat.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "IsChecked")
                        {
                            OnPropertyChanged("SelectedCategories");
                            OnPropertyChanged("AllEmpty");
                        }
                    });
                    _categories = new List<HrCategoryViewModel>(catsVM);
                }
                return _categories;
            }
        }

        public DateOffset HrDateOffsetLower
        {
            get
            {
                if (_hrDateOffsetLower == null)
                {
                    _hrDateOffsetLower = new DateOffset(null, DateUnits.Day);
                    _hrDateOffsetLower.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "Offset" || e.PropertyName == "Unit")
                        {
                            OnPropertyChanged("HrDateOffsetLower");
                            OnPropertyChanged("AllEmpty");
                            PrintHrDate();
                        }
                    };
                }
                return _hrDateOffsetLower;
            }
            private set
            {
                _hrDateOffsetLower.Offset = value.Offset;
                _hrDateOffsetLower.Unit = value.Unit;
            }
        }

        public DateOffset HrDateOffsetUpper
        {
            get
            {
                if (_hrDateOffsetUpper == null)
                {
                    _hrDateOffsetUpper = new DateOffset(null, DateUnits.Day);
                    _hrDateOffsetUpper.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "Offset" || e.PropertyName == "Unit")
                        {
                            OnPropertyChanged("HrDateOffsetUpper");
                            OnPropertyChanged("AllEmpty");
                            PrintHrDate();
                        }
                    };
                }
                return _hrDateOffsetUpper;
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

        public int AndScope
        {
            get
            {
                return _allSCope;
            }
            set
            {
                if (_allSCope != value)
                {
                    _allSCope = value;
                    OnPropertyChanged(() => AndScope);
                }
            }
        }


        #endregion Options bindings

        public IList<HrCategoryViewModel> SelectedCategories
        {
            get { return Categories.Where(cat => cat.IsChecked).ToList(); }
        }

        public DateTime? AppDateGt
        {
            get
            {
                return DateHelper.NullableDate(AppYearLower, AppMonthLower, AppDayLower);
            }
        }

        public DateTime? AppDateLt
        {
            get
            {
                return DateHelper.NullableDate(AppYearUpper, AppMonthUpper, AppDayUpper);
            }
        }

        public bool AllEmpty
        {
            get
            {
                return AppDateGt == null && AppDateLt == null
                    && (HrDateOffsetLower.IsEmpty || HrDateOffsetUpper.IsEmpty)
                    && SelectedCategories.Count() == 0
                    && Autocomplete.Tags.Count == 1;
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return new RelayCommand(Search, () => !AllEmpty);
            }
        }

        public Autocomplete Autocomplete { get; private set; }


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
                return Result != null && Result.Statistic.Count == 0;
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

        private HrSearchOptions MakeOptions()
        {
            var options = new HrSearchOptions();

            options.HealthRecordOffsetGt = HrDateOffsetLower;
            options.HealthRecordOffsetLt = HrDateOffsetUpper;
            options.AppointmentDateGt = DateHelper.NullableDate(AppYearLower, AppMonthLower, AppDayLower);
            options.AppointmentDateLt = DateHelper.NullableDate(AppYearUpper, AppMonthUpper, AppDayUpper);
            options.AllWords = AllWords;
            options.AndScope = (HealthRecordQuery.AndScopes)AndScope;

            var entities = Autocomplete.GetEntities().ToList();
            options.Words = entities.Where(x => x is Word).Cast<Word>().ToList();
            options.Categories = SelectedCategories.Select(cat => cat.category).ToList();

            Options = options;
            return options;
        }

        private void Search()
        {
            MakeOptions();
            var hrs = searcher.Search(Session, Options);
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

            // комментарий и давность из последней записи
            var lastHr = hrs.Last();
            HrDateOffsetLower = new DateOffset(lastHr.FromYear, lastHr.FromMonth, lastHr.FromDay);

            RecieveHrItemObjects(allWords);
        }

        private void RecieveHrItemObjects(IEnumerable<IHrItemObject> hios)
        {
            Autocomplete.ReplaceTagsWith(hios);

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

        private void PrintHrDate()
        {
            Debug.Print("HrDateOffsetUpper = {0}", HrDateOffsetUpper);
            Debug.Print("HrDateOffsetLower = {0}", HrDateOffsetLower);
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                msgManager.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable

        public class HrCategoryViewModel : CheckableBase, IComparable
        {
            internal readonly HrCategory category;

            public string Name
            {
                get
                {
                    return category.Name;
                }
            }

            public HrCategoryViewModel(HrCategory category)
            {
                Contract.Requires(category != null);
                this.category = category;
            }

            public int CompareTo(object obj)
            {
                if (obj == null)
                    return -1;

                HrCategoryViewModel other = obj as HrCategoryViewModel;
                if (other != null)
                {
                    return this.category.Ord.CompareTo(other.category.Ord);
                }
                else
                    throw new ArgumentException("Object is not a CategoryViewModel");
            }

            public override string ToString()
            {
                return category.ToString();
            }
        }
    }
}