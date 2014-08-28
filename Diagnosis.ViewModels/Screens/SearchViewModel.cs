﻿using Diagnosis.Core;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        #region Fields

        private HrSearchOptions _options;

        private bool _any;
        private int? _appDayLower;
        private int? _appDayUpper;
        private int? _appMonthLower;
        private int? _appMonthUpper;
        private int? _appYearLower;
        private int? _appYearUpper;
        private string _comment;
        private DateOffset _hrDateOffsetLower;
        private DateOffset _hrDateOffsetUpper;
        private bool _controlsVisible;

        private IEnumerable<CategoryViewModel> _categories;
        private bool _searchWas;
        private ICategoryRepository catRepo;
        private PatientsProducer patManager;
        private HrSearcher searcher = new HrSearcher();

        private EventMessageHandlersManager msgManager;

        #endregion Fields

        public SearchViewModel(PatientsProducer manager)
        {
            Contract.Requires(manager != null);
            patManager = manager;

            WordSearch = new WordRootAutoComplete(QuerySeparator.Default, new HierarchicalSearchSettings());
            Results = new ObservableCollection<HrSearchResultViewModel>();
            ControlsVisible = true;
            AnyWord = true;

            catRepo = new CategoryRepository();

            WordSearch.InputEnded += (s, e) =>
            {
                Search();
            };
            ((INotifyCollectionChanged)Words).CollectionChanged += (s, e) =>
            {
                OnPropertyChanged("AllEmpty");
            };
            msgManager = new EventMessageHandlersManager(
                this.Subscribe(Events.SendToSearch, (e) =>
                {
                    try
                    {
                        var hrs = e.GetValue<IEnumerable<HealthRecordViewModel>>(MessageKeys.HealthRecord);
                        if (hrs != null && hrs.Count() > 0)
                        {
                            RecieveHealthRecords(hrs);
                        }
                    }
                    catch { }
                    try
                    {
                        var words = e.GetValue<IEnumerable<WordViewModel>>(MessageKeys.Word);
                        if (words != null && words.Count() > 0)
                        {
                            RecieveWords(words);
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

        public IEnumerable<CategoryViewModel> Categories
        {
            get
            {
                if (_categories == null)
                {
                    var catsVM = catRepo.GetAll().Select(cat => new CategoryViewModel(cat)).ToList();
                    catsVM.ForAll(cat => cat.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "IsChecked")
                        {
                            OnPropertyChanged("SelectedCategories");
                            OnPropertyChanged("AllEmpty");
                        }
                    });
                    _categories = new List<CategoryViewModel>(catsVM);
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

        public bool AnyWord
        {
            get
            {
                return _any;
            }
            set
            {
                if (_any != value)
                {
                    _any = value;
                    OnPropertyChanged("AnyWord");
                }
            }
        }

        public ReadOnlyObservableCollection<WordViewModel> Words { get { return WordSearch.Items; } }

        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    OnPropertyChanged("Comment");
                    OnPropertyChanged("CommentVisible");
                    OnPropertyChanged("AllEmpty");
                }
            }
        }

        #endregion Options bindings

        public bool SearchWas
        {
            get { return _searchWas; }
            set
            {
                _searchWas = value;
                OnPropertyChanged("SearchWas");
            }
        }

        public IList<CategoryViewModel> SelectedCategories
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
                    && Words.Count == 0
                    && string.IsNullOrEmpty(Comment);
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return new RelayCommand(Search, () => !AllEmpty);
            }
        }

        public RelayCommand<Patient> OpenPatientCommand
        {
            get
            {
                return new RelayCommand<Patient>(
                                          p =>
                                          {
                                              this.Send(Events.OpenPatient, patManager.GetByModel(p).AsParams(MessageKeys.Patient));
                                          });
            }
        }

        public AutoCompleteBase<WordViewModel> WordSearch
        {
            get;
            private set;
        }

        public ObservableCollection<HrSearchResultViewModel> Results
        {
            get;
            private set;
        }
        /// <summary>
        /// Показывать сообщение «ничего не найдено»
        /// </summary>
        public bool NoResultsVisible
        {
            get
            {
                return Results.Count == 0 && SearchWas;
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
            options.AnyWord = AnyWord;
            options.Words = Words;
            options.Categories = SelectedCategories.ToList();
            options.Comment = Comment;

            Options = options;
            return options;
        }

        private void Search()
        {
            Results.Clear();

            MakeOptions();
            var hrs = searcher.Search(Options);
            // только одна запись из осмотра
            hrs.Distinct(new KeyEqualityComparer<HealthRecord, Appointment>(hr => hr.Appointment))
                .ForAll(hr => Results.Add(new HrSearchResultViewModel(hr, Options)));

            SearchWas = true;
            ControlsVisible = false;

            OnPropertyChanged("NoResultsVisible");
        }

        private void RecieveHealthRecords(IEnumerable<HealthRecordViewModel> hrs)
        {
            // все слова из записей
            var allWords = hrs.Aggregate(new HashSet<WordViewModel>(), (words, hr) => { hr.Symptom.Words.ForAll((w) => words.Add(w)); return words; });
            WordSearch.Reset(allWords);

            // если несколько записей — любое из слов
            AnyWord = hrs.Count() != 1;

            // все категории из записей
            Categories.ForAll((cat) => cat.IsChecked = false);
            var allCats = hrs.Aggregate(new HashSet<Category>(), (cats, hr) => { cats.Add(hr.Category); return cats; });
            Categories.Where(cat => allCats.Contains(cat.category)).ForAll(cat => cat.IsChecked = true);

            // комментарий и давность из последней записи
            var lastHr = hrs.Last();
            HrDateOffsetLower = lastHr.DateOffset;
            Comment = lastHr.Comment;


            RemoveLastResults();
        }

        private void RecieveWords(IEnumerable<WordViewModel> words)
        {
            // ищем переданные слова
            WordSearch.Reset(words);

            RemoveLastResults();
        }

        /// <summary>
        /// Убирает результаты предыдущего поиска
        /// </summary>
        private void RemoveLastResults()
        {
            Results.Clear();
            SearchWas = false;
            ControlsVisible = true;
        }

        private void PrintHrDate()
        {
            Debug.Print("HrDateOffsetUpper = {0}", HrDateOffsetUpper);
            Debug.Print("HrDateOffsetLower = {0}", HrDateOffsetLower);
        }

        #region IDisposable

        private bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    msgManager.Dispose();
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable
    }
}