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
    public class QueryBlockViewModel : ToolViewModel
    {
        private bool _all;
        private HealthRecordQueryAndScope _scope;
        private HrSearchOptions _options;
        private IList<HrCategoryViewModel> _categories;

        public QueryBlockViewModel()
        {
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

            AutocompleteAll.InputEnded += AutocompleteAll_InputEnded;
            AutocompleteAny.InputEnded += AutocompleteAll_InputEnded;
            AutocompleteNot.InputEnded += AutocompleteAll_InputEnded;
            AutocompleteAll.Tags.CollectionChanged += Tags_CollectionChanged;
            AutocompleteAny.Tags.CollectionChanged += Tags_CollectionChanged;
            AutocompleteNot.Tags.CollectionChanged += Tags_CollectionChanged;

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


        public AutocompleteViewModel AutocompleteAll { get; set; }

        public AutocompleteViewModel AutocompleteAny { get; set; }

        public AutocompleteViewModel AutocompleteNot { get; set; }

        public SearchViewModel Search { get; set; }

        /// <summary>
        /// Опции поиска при последнем поиске.
        /// </summary>
        public HrSearchOptions Options
        {
            get { return _options; }
            private set
            {
                _options = value;
                OnPropertyChanged("Options");
            }
        }

        public IList<HrCategoryViewModel> Categories
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
                            OnPropertyChanged("AllEmpty"); //
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

        public RelayCommand RemoveQbCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Search.QueryBlocks.Remove(this);
                });
            }
        }

        public HrSearchOptions MakeOptions()
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AutocompleteAll.InputEnded -= AutocompleteAll_InputEnded;
                AutocompleteAny.InputEnded -= AutocompleteAll_InputEnded;
                AutocompleteNot.InputEnded -= AutocompleteAll_InputEnded;
                AutocompleteAll.Tags.CollectionChanged -= Tags_CollectionChanged;
                AutocompleteAny.Tags.CollectionChanged -= Tags_CollectionChanged;
                AutocompleteNot.Tags.CollectionChanged -= Tags_CollectionChanged;
            }
            base.Dispose(disposing);
        }

        void AutocompleteAll_InputEnded(object sender, BoolEventArgs e)
        {
            if (Search.SearchCommand.CanExecute(null))
                Search.SearchCommand.Execute(null);
        }

        void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested(); // when drop tag, search button still disabled
            OnPropertyChanged("AllEmpty");
        }
    }
}