using Diagnosis.Core;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        private bool _any;
        private int? _appDayLower;
        private int? _appDayUpper;
        private int? _appMonthLower;
        private int? _appMonthUpper;
        private int? _appYearLower;
        private int? _appYearUpper;
        private DateOffset _hrDateOffsetLower;
        private DateOffset _hrDateOffsetUpper;
        private RelayCommand _searchCommand;
        private bool _controlsVisible;

        private bool searchWas;
        private HrSearcher searcher = new HrSearcher();

        public SearchViewModel()
        {
            WordSearch = new WordCheckingAutoComplete(QuerySeparator.Default, new SearcherSettings() { AllChildren = true });
            Words = new ObservableCollection<WordViewModel>();
            Results = new ObservableCollection<SearchResultViewModel>();
            ControlsVisible = true;

            this.Subscribe((int)EventID.WordCheckedChanged, (e) =>
            {
                var word = e.GetValue<WordViewModel>(Messages.Word);
                var isChecked = e.GetValue<bool>(Messages.CheckedState);

                OnWordCheckedChanged(word, isChecked);
            });
            this.Subscribe((int)EventID.CategoryCheckedChanged, (e) =>
            {
                OnPropertyChanged(() => SelectedCategories);
            });
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

                    OnPropertyChanged(() => AppDayLower);
                    OnPropertyChanged(() => AppDateVisible);
                    OnPropertyChanged(() => AppDateGt);
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

                    OnPropertyChanged(() => AppDayUpper);
                    OnPropertyChanged(() => AppDateVisible);
                    OnPropertyChanged(() => AppDateLt);
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

                    OnPropertyChanged(() => AppMonthLower);
                    OnPropertyChanged(() => AppDateVisible);
                    OnPropertyChanged(() => AppDateGt);
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

                    OnPropertyChanged(() => AppMonthUpper);
                    OnPropertyChanged(() => AppDateVisible);
                    OnPropertyChanged(() => AppDateLt);
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

                    OnPropertyChanged(() => AppYearLower);
                    OnPropertyChanged(() => AppDateVisible);
                    OnPropertyChanged(() => AppDateGt);
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

                    OnPropertyChanged(() => AppYearUpper);
                    OnPropertyChanged(() => AppDateVisible);
                    OnPropertyChanged(() => AppDateLt);
                }
            }
        }

        public ObservableCollection<CategoryViewModel> Categories
        {
            get
            {
                return EntityManagers.CategoryManager.Categories;
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
                            OnPropertyChanged(() => HrDateOffsetLower);
                            OnPropertyChanged(() => HrDateVisible);
                            OnPropertyChanged(() => HrDateLt);
                            OnPropertyChanged(() => HrDateGt);
                            PrintHrDate();
                        }
                    };
                }
                return _hrDateOffsetLower;
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
                            OnPropertyChanged(() => HrDateOffsetUpper);
                            OnPropertyChanged(() => HrDateVisible);
                            OnPropertyChanged(() => HrDateLt);
                            OnPropertyChanged(() => HrDateGt);
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
                    OnPropertyChanged(() => AnyWord);
                }
            }
        }

        public ObservableCollection<WordViewModel> Words
        {
            get;
            private set;
        }

        #endregion

        #region Options results

        public IEnumerable<CategoryViewModel> SelectedCategories
        {
            get { return Categories.Where(cat => cat.IsChecked); }
        }
        /// <summary>
        /// Дата приёма, для поиска достаточно любой границы.
        /// </summary>
        public bool AppDateVisible
        {
            get
            {
                return AppDateGt != null || AppDateLt != null;
            }
        }

        public DateTime? AppDateGt
        {
            get
            {
                return DateOffset.NullableDate(AppYearLower, AppMonthLower, AppDayLower);
            }
        }
        public DateTime? AppDateLt
        {
            get
            {
                return DateOffset.NullableDate(AppYearUpper, AppMonthUpper, AppDayUpper);
            }
        }
        /// <summary>
        /// Давность признака в записи между двумя обязательными значениями.
        /// </summary>
        public bool HrDateVisible
        {
            get
            {
                return HrDateGt.Offset != null && HrDateLt.Offset != null;
            }
        }

        // границы интервала давности могут быть введены в любом порядке

        public DateOffset HrDateGt
        {
            get
            {
                return HrDateOffsetLower < HrDateOffsetUpper ? HrDateOffsetLower : HrDateOffsetUpper;
            }
        }
        public DateOffset HrDateLt
        {
            get
            {
                return HrDateOffsetUpper > HrDateOffsetLower ? HrDateOffsetUpper : HrDateOffsetLower;
            }
        }

        #endregion

        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand
                    ?? (_searchCommand = new RelayCommand(
                                          () =>
                                          {
                                              Results.Clear();
                                              var options = GetOptions();
                                              searcher.Search(options).
                                                  ForAll(hr => Results.Add(new SearchResultViewModel(hr, options)));

                                              searchWas = true;
                                              OnPropertyChanged(() => NoResultsVisible);
                                          }, () => Words.Count > 0));
            }
        }

        public AutoCompleteBase<WordViewModel> WordSearch
        {
            get;
            private set;
        }

        public ObservableCollection<SearchResultViewModel> Results
        {
            get;
            private set;
        }
        public bool NoResultsVisible
        {
            get
            {
                return Results.Count == 0 && searchWas;
            }
        }
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
                    OnPropertyChanged(() => ControlsVisible);
                }
            }
        }

        private HrSearchOptions GetOptions()
        {
            var options = new HrSearchOptions();

            options.HealthRecordFromDateGt = HrDateGt;
            options.HealthRecordFromDateLt = HrDateLt;
            options.AppointmentDateGt = AppDateGt;
            options.AppointmentDateLt = AppDateLt;
            options.AnyWord = AnyWord;
            options.Words = Words;
            options.Categories = SelectedCategories.ToList();

            return options;
        }

        private void OnWordCheckedChanged(WordViewModel word, bool isChecked)
        {
            if (isChecked)
            {
                Words.Add(word);
            }
            else
            {
                Words.Remove(word);
            }
        }

        void PrintHrDate()
        {
            Console.WriteLine("HrDateOffsetUpper = {0}", HrDateOffsetUpper);
            Console.WriteLine("HrDateOffsetLower = {0}", HrDateOffsetLower);
            Console.WriteLine("HrDateLt = {0}", HrDateLt);
            Console.WriteLine("HrDateGt = {0}", HrDateGt);
        }
    }
}