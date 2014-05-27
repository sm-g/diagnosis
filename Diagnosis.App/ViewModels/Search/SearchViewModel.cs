using Diagnosis.Core;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
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

        private bool searchWas;
        private HrSearcher searcher = new HrSearcher();

        public SearchViewModel()
        {
            WordSearch = new WordAutoComplete(QuerySeparator.Default, new SearcherSettings() { AllChildren = true });
            Words = new ObservableCollection<WordViewModel>();
            Results = new ObservableCollection<HealthRecordViewModel>();

            this.Subscribe((int)EventID.WordCheckedChanged, (e) =>
            {
                var word = e.GetValue<WordViewModel>(Messages.Word);
                var isChecked = e.GetValue<bool>(Messages.CheckedState);

                OnWordCheckedChanged(word, isChecked);
            });
        }

        #region Options

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
                return _hrDateOffsetLower ?? (_hrDateOffsetLower = new DateOffset(null, DateUnits.Day));
            }
            set
            {
                if (_hrDateOffsetLower != value)
                {
                    _hrDateOffsetLower = value;
                    OnPropertyChanged(() => HrDateOffsetLower);
                }
            }
        }

        public DateOffset HrDateOffsetUpper
        {
            get
            {
                return _hrDateOffsetUpper ?? (_hrDateOffsetUpper = new DateOffset(null, DateUnits.Day));
            }
            set
            {
                if (_hrDateOffsetUpper != value)
                {
                    _hrDateOffsetUpper = value;
                    OnPropertyChanged(() => HrDateOffsetUpper);
                }
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

        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand
                    ?? (_searchCommand = new RelayCommand(
                                          () =>
                                          {
                                              Results.Clear();
                                              searcher.Search(GetOptions()).
                                                  ForAll(hr => Results.Add(new HealthRecordViewModel(hr)));

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

        public ObservableCollection<HealthRecordViewModel> Results
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

        private HrSearchOptions GetOptions()
        {
            var options = new HrSearchOptions();

            if (HrDateOffsetLower != null && HrDateOffsetUpper != null)
            {
                // границы интервала давности могут быть введены в любом порядке
                options.HealthRecordFromDateGt = HrDateOffsetLower < HrDateOffsetUpper ? HrDateOffsetLower : HrDateOffsetUpper;
                options.HealthRecordFromDateLt = HrDateOffsetUpper > HrDateOffsetLower ? HrDateOffsetUpper : HrDateOffsetLower;
            }
            options.AppointmentDateGt = DateOffset.NullableDate(AppYearLower, AppMonthLower, AppDayLower);
            options.AppointmentDateLt = DateOffset.NullableDate(AppYearUpper, AppMonthUpper, AppDayUpper);
            options.AnyWord = AnyWord;
            options.Words = Words;
            options.Categories = Categories.Where(cat => cat.IsChecked);

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
    }
}