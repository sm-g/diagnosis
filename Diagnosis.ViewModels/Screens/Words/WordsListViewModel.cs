using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using EventAggregator;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using NHibernate.Linq;
using System.Windows.Input;
using Diagnosis.Data;

namespace Diagnosis.ViewModels.Screens
{
    public class WordsListViewModel : ScreenBase
    {
        private FilterViewModel<Word> _filter;
        private ObservableCollection<WordViewModel> _words;
        private bool _noWords;
        private WordViewModel _current;
        EventMessageHandlersManager emhManager;
        private Saver saver;
        public WordsListViewModel()
        {
            _filter = new FilterViewModel<Word>(WordQuery.StartingWith(Session));
            saver = new Saver(Session);
            SelectedWords = new ObservableCollection<WordViewModel>();

            Filter.Filtered += (s, e) =>
            {
                MakeVms(Filter.Results);
            };
            Filter.Clear(); // показываем все

            emhManager = new EventMessageHandlersManager(new[] {
                this.Subscribe(Events.WordSaved, (e) =>
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
                this.Subscribe(Events.DeleteWord, (e) =>
                {
                    var w = e.GetValue<Word>(MessageKeys.Word);
                    saver.Delete(w);

                    // убираем удаленных из списка
                    Filter.Filter();

                    NoWords = !Session.Query<Word>().Any();
                })
            });

            Title = "Словарь";
            NoWords = !Session.Query<Patient>().Any();
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
                    var patientsView = (CollectionView)CollectionViewSource.GetDefaultView(_words);
                    SortDescription sort1 = new SortDescription("Title", ListSortDirection.Ascending);
                    patientsView.SortDescriptions.Add(sort1);
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
                            var word = new Word(Filter.Query);
                            this.Send(Events.EditWord, word.AsParams(MessageKeys.Word));
                        });
            }
        }
        public ICommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Events.EditWord, SelectedWord.word.AsParams(MessageKeys.Word));
                }, () => SelectedWord != null);
            }
        }
        public ICommand SendToSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            this.Send(Events.SendToSearch, Words.Where(w => w.IsChecked)
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
                        .Where(p => p.DeleteCommand.CanExecute(null))
                        .ToArray();

                    //

                }, () => SelectedWords.Any(p => p.DeleteCommand.CanExecute(null)));
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
        /// В БД нет слов.
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
        private void MakeVms(ObservableCollection<Word> results)
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
            }
            base.Dispose(disposing);
        }
    }
}