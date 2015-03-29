using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Controls;
using Diagnosis.ViewModels.Search;
using EventAggregator;
using NHibernate.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class WordsListViewModel : ScreenBaseViewModel, IFilterableList
    {
        private FilterViewModel<Word> _filter;
        private ObservableCollection<WordViewModel> _words;
        private bool _noWords;
        private WordViewModel _current;
        private EventMessageHandlersManager emhManager;
        private Saver saver;
        Doctor doctor;

        public WordsListViewModel()
        {
            _filter = new FilterViewModel<Word>(WordQuery.StartingWith(Session));
            saver = new Saver(Session);
            doctor = AuthorityController.CurrentDoctor;

            SelectedWords = new ObservableCollection<WordViewModel>();

            Filter.Filtered += (s, e) =>
            {
                // показываем только слова, доступные врачу
                MakeVms(Filter.Results
                    .Where(x => doctor.Words.Contains(x)));
            };
            Filter.Clear(); // показываем все

            emhManager = new EventMessageHandlersManager(new[] {
                this.Subscribe(Event.WordSaved, (e) =>
                {
                    // новое слово или изменившееся с учетом фильтра
                    Filter.Filter();
                    var saved = e.GetValue<Word>(MessageKeys.Word);

                    // сохраненное слово с таким текстом
                    var persisted = WordQuery.ByTitle(Session)(saved.Title);

                    var vm = Words.Where(w => w.word == persisted).FirstOrDefault();
                    if (vm != null)
                        vm.IsSelected = true;

                    NoWords = false;
                }),
               
            });

            Title = "Словарь";
            NoWords = Words.Count == 0;
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
                return _current;
            }
            set
            {
                if (_current != value)
                {
                    _current = value;
                    OnPropertyChanged(() => SelectedWord);
                }
            }
        }

        public ObservableCollection<WordViewModel> SelectedWords { get; private set; }

        public RelayCommand<WordViewModel> AddCommand
        {
            get
            {
                return new RelayCommand<WordViewModel>((current) =>
                        {
                            var title = Filter.Query;
                            if (SelectedWord != null)
                                title = SelectedWord.Title;

                            var word = new Word(title);
                            this.Send(Event.EditWord, word.AsParams(MessageKeys.Word));
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
                                .Select(w => w.word)
                                .AsParams(MessageKeys.HrItemObjects));
                        }, () => CheckedWordsCount > 0);
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var toDel = SelectedWords
                        .Select(w => w.word)
                        .Where(w => w.IsEmpty())
                        .ToArray();

                    // todo не просто удалять, а убирать для этого врача
                    toDel.ForAll(x => x.OnDelete());
                    saver.Delete(toDel);

                    // убираем удаленных из списка
                    Filter.Filter();

                    NoWords = Words.Count == 0;
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
                return _noWords;
            }
            set
            {
                if (_noWords != value)
                {
                    _noWords = value;
                    OnPropertyChanged(() => NoWords);
                }
            }
        }

        private void MakeVms(IEnumerable<Word> results)
        {
            var vms = results.Select(w => Words
                .Where(vm => vm.word == w)
                .FirstOrDefault() ?? new WordViewModel(w));

            Words.SyncWith(vms);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                emhManager.Dispose();
                _filter.Dispose();
            }
            base.Dispose(disposing);
        }

        internal void SelectWord(Word w)
        {
            var toSelect = Words.FirstOrDefault(vm => vm.word == w);
            SelectedWord = toSelect;
            if (!SelectedWords.Contains(toSelect))
                SelectedWords.Add(toSelect);

        }
    }
}