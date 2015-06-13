using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Controls;
using EventAggregator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class WordsListViewModel : ScreenBaseViewModel, IFilterableList
    {
        private FilterViewModel<Word> _filter;
        private ObservableCollection<WordViewModel> _words;
        private WordViewModel _current;
        private Saver saver;
        private Doctor doctor;
        private FilterableListHelper<Word, WordViewModel> filterHelper;

        public WordsListViewModel()
        {
            saver = new Saver(Session);
            doctor = AuthorityController.CurrentDoctor;

            _filter = new FilterViewModel<Word>(WordQuery.StartingWith(Session));
            Filter.Filtered += (s, e) =>
            {
                // показываем только слова, доступные врачу
                MakeVms(Filter.Results.Where(x => doctor.Words.Contains(x)));
            };

            filterHelper = new FilterableListHelper<Word, WordViewModel>(this, (v) => v.word);
            filterHelper.AddSelectVmWithEntityOn(Event.WordSaved, MessageKeys.Word, () => OnPropertyChanged(() => NoWords));

            Title = "Словарь";
            Filter.Clear(); // показываем все
        }

        public FilterViewModel<Word> Filter
        {
            get { return _filter; }
        }

        public ObservableCollection<WordViewModel> Words
        {
            get
            {
                if (_words == null)
                {
                    _words = new ObservableCollection<WordViewModel>();
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(_words);
                    SortDescription sort1 = new SortDescription("Title", ListSortDirection.Ascending);
                    view.SortDescriptions.Add(sort1);
                }
                return _words;
            }
        }

        public WordViewModel SelectedWord
        {
            get
            {
                Contract.Ensures(Contract.Result<CheckableBase>() == null || Contract.Result<CheckableBase>().IsSelected);
                return _current;
            }
            set
            {
                if (_current != value)
                {
                    _current = value;
                    if (_current != null)
                        _current.IsSelected = true;
                    OnPropertyChanged(() => SelectedWord);
                }
            }
        }

        public IEnumerable<WordViewModel> SelectedWords { get { return Words.Where(x => x.IsSelected); } }

        public RelayCommand<WordViewModel> AddCommand
        {
            get
            {
                return new RelayCommand<WordViewModel>((current) =>
                        {
                            var title = Filter.Query;
                            if (SelectedWord != null)
                                title = SelectedWord.Title;

                            AddWord(title);
                        });
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.EditWord, SelectedWord.word.AsParams(MessageKeys.Word));
                }, () => SelectedWord != null);
            }
        }

        public ICommand SendToSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            this.Send(Event.SendToSearch, Words.Where(w => w.IsChecked)
                                .Select(w => w.word.AsConfidencable())
                                .AsParams(MessageKeys.ToSearchPackage));
                        }, () => CheckedWordsCount > 0);
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    // можно удалить неиспользованные любым врачом слова
                    var toDel = SelectedWords
                        .Select(w => w.word)
                        .Where(w => w.IsEmpty())
                        .ToArray();

                    toDel.ForAll(x => x.OnDelete());
                    saver.Delete(toDel);

                    // убираем удаленные из списка
                    Filter.Filter();

                    OnPropertyChanged(() => NoWords);
                }, () => SelectedWords.Any(w => w.word.IsEmpty()));
            }
        }

        public int CheckedWordsCount
        {
            get
            {
                return Words.Where(w => w.IsChecked).Count();
            }
        }

        /// <summary>
        /// Нет слов, доступных врачу.
        /// </summary>
        public bool NoWords
        {
            get
            {
                return doctor.Words.Count() == 0;
            }
        }

        IFilter IFilterableList.Filter
        {
            get { return Filter; }
        }

        IEnumerable<CheckableBase> IFilterableList.Items
        {
            get { return Words.Cast<CheckableBase>(); }
        }

        internal Word AddWord(string title)
        {
            Contract.Requires(title != null);

            var word = new Word(title);
            // use created word if possible
            word = CreatedWordsManager.GetSameWordFromCreated(word) ?? word;

            this.Send(Event.EditWord, word.AsParams(MessageKeys.Word));
            return word;
        }

        internal void SelectWord(Word w)
        {
            var toSelect = Words.FirstOrDefault(vm => vm.word == w);
            SelectedWord = toSelect;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _filter.Dispose();
                filterHelper.Dispose();
            }
            base.Dispose(disposing);
        }

        private void MakeVms(IEnumerable<Word> results)
        {
            var vms = results.Select(w => Words
                .Where(vm => vm.word == w)
                .FirstOrDefault() ?? new WordViewModel(w));

            Words.SyncWith(vms);
        }
    }
}