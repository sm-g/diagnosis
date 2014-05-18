using Diagnosis.Core;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        private bool _any;
        private int _appDayLower;

        private int _appDayUpper;

        private int _appMonthLower;

        private int _appMonthUpper;

        private int _appYearLower;

        private int _appYearUpper;

        private bool _categoryMultiSelection;

        private DateOffset _hrDateOffsetLower;

        private DateOffset _hrDateOffsetUpper;

        private RelayCommand _searchCommand;

        public SearchViewModel()
        {
            WordSearch = new WordAutoComplete();
            Words = new ObservableCollection<WordViewModel>();
            Results = new ObservableCollection<HealthRecordViewModel>();

            this.Subscribe((int)EventID.WordCheckedChanged, (e) =>
            {
                var word = e.GetValue<WordViewModel>(Messages.Word);
                var isChecked = e.GetValue<bool>(Messages.CheckedState);

                OnWordCheckedChanged(word, isChecked);
            });
        }

        public int AppDayLower
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

        public int AppDayUpper
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

        public int AppMonthLower
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

        public int AppMonthUpper
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

        public int AppYearLower
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

        public int AppYearUpper
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

        public bool CategoryMultiSelection
        {
            get
            {
                return _categoryMultiSelection;
            }
            set
            {
                if (_categoryMultiSelection != value)
                {
                    _categoryMultiSelection = value;
                    OnPropertyChanged(() => CategoryMultiSelection);
                }
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

        public ObservableCollection<HealthRecordViewModel> Results
        {
            get;
            private set;
        }

        private HrSearcher searcher = new HrSearcher();

        private HrSearchOptions GetOptions()
        {
            var options = new HrSearchOptions();

            if (HrDateOffsetLower != null && HrDateOffsetUpper != null)
            {
                // границы интервала давности могут быть введены в любом порядке
                options.HealthRecordFromDateGt = HrDateOffsetLower < HrDateOffsetUpper ? HrDateOffsetLower : HrDateOffsetUpper;
                options.HealthRecordFromDateLt = HrDateOffsetUpper > HrDateOffsetUpper ? HrDateOffsetUpper : HrDateOffsetLower;
            }
            else
            {
                options.HealthRecordFromDateGt = HrDateOffsetLower;
                options.HealthRecordFromDateLt = HrDateOffsetUpper;
            }
            options.AppointmentDateGt = NullableDate(AppYearLower, AppMonthLower, AppDayLower);
            options.AppointmentDateLt = NullableDate(AppYearUpper, AppMonthUpper, AppDayUpper);
            options.AnyWord = AnyWord;
            options.Words = Words;

            return options;
        }

        private static DateTime? NullableDate(int year, int month, int day)
        {
            try
            {
                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand
                    ?? (_searchCommand = new RelayCommand(
                                          () =>
                                          {
                                              searcher.Search(GetOptions()).
                                                  ForAll(hr => Results.Add(new HealthRecordViewModel(hr)));
                                          }));
            }
        }

        public ObservableCollection<WordViewModel> Words
        {
            get;
            private set;
        }

        public WordAutoComplete WordSearch
        {
            get;
            private set;
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