using Diagnosis.App.Messaging;
using Diagnosis.Core;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
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
        private RelayCommand _searchCommand;
        private RelayCommand<PatientViewModel> _openPatientCommand;
        private bool _controlsVisible;

        private bool _searchWas;
        private HrSearcher searcher = new HrSearcher();

        #endregion Fields

        public SearchViewModel()
        {
            WordSearch = new WordRootAutoComplete(QuerySeparator.Default, new SimpleSearcherSettings() { AllChildren = true });
            Results = new ObservableCollection<HrSearchResultViewModel>();
            ControlsVisible = true;
            AnyWord = true;

            this.Subscribe((int)EventID.CategoryCheckedChanged, (e) =>
            {
                OnPropertyChanged("SelectedCategories");
                OnPropertyChanged("AllEmpty");
            });
            ((INotifyCollectionChanged)Words).CollectionChanged += (s, e) =>
            {
                OnPropertyChanged("AllEmpty");
            };
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
                            OnPropertyChanged("HrDateOffsetLower");
                            OnPropertyChanged("AllEmpty");
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

        #region AllEmpty

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

        #endregion Options results
        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand
                    ?? (_searchCommand = new RelayCommand(
                                          () =>
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
                                          }, () => !AllEmpty));
            }
        }

        public ICommand OpenPatientCommand
        {
            get
            {
                return _openPatientCommand
                    ?? (_openPatientCommand = new RelayCommand<PatientViewModel>(
                                          p =>
                                          {
                                              this.Send((int)EventID.OpenPatient, new PatientParams(p).Params);
                                          }));
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

        public bool NoResultsVisible
        {
            get
            {
                return Results.Count == 0 && SearchWas;
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
            options.AppointmentDateGt = DateOffset.NullableDate(AppYearLower, AppMonthLower, AppDayLower);
            options.AppointmentDateLt = DateOffset.NullableDate(AppYearUpper, AppMonthUpper, AppDayUpper);
            options.AnyWord = AnyWord;
            options.Words = Words;
            options.Categories = SelectedCategories.ToList();
            options.Comment = Comment;

            Options = options;
            return options;
        }

        private void PrintHrDate()
        {
            Console.WriteLine("HrDateOffsetUpper = {0}", HrDateOffsetUpper);
            Console.WriteLine("HrDateOffsetLower = {0}", HrDateOffsetLower);
        }
    }
}