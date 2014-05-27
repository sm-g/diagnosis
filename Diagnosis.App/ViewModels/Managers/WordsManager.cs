using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class WordsManager : ViewModelBase
    {
        private IWordRepository repository;
        private RelayCommand _commit;
        WordSearcher _searcher;

        public ObservableCollection<WordViewModel> Words
        {
            get;
            private set;
        }
        /// <summary>
        /// Поисковик по всем словам, создает новые из запроса.
        /// </summary>
        public WordSearcher RootSearcher
        {
            get
            {
                return _searcher ?? (_searcher = new WordSearcher(Root,
                    new SearcherSettings() { WithChecked = true }));
            }
        }

        private IEnumerable<WordViewModel> DirtyWords
        {
            get
            {
                return Words.Where(s => s.Editable.IsDirty);
            }
        }

        WordViewModel Root { get; set; }

        public ICommand CommitCommand
        {
            get
            {
                return _commit
                    ?? (_commit = new RelayCommand(
                                          () =>
                                          {
                                              foreach (var item in DirtyWords)
                                              {
                                                  if (item.IsChecked)
                                                      item.Editable.CommitCommand.Execute(null);
                                              }
                                          },
                                          () => DirtyWords.Count() > 0));
            }
        }

        public IEnumerable<WordViewModel> GetSymptomWords(Symptom s)
        {
            Contract.Requires(s != null);

            if (Words.Count > 0)
            {
                var intersect = Root.AllChildren.Select(w => w.word).Intersect(s.Words);

                return Root.AllChildren.Where(w => intersect.Contains(w.word));
            }

            return Enumerable.Empty<WordViewModel>();
        }

        public void CheckThese(IEnumerable<WordViewModel> words)
        {
            UnCheckAll();

            foreach (var item in words)
            {
                item.IsChecked = true;
            }
        }
        /// <summary>
        /// Создает слово и добавляет в коллекцию Words, если требуется.
        /// </summary>
        public WordViewModel Create(string title)
        {
            var existing = Find(title);

            if (existing != null)
                return existing;

            var vm = new WordViewModel(new Word(title));
            vm.Editable.MarkDirty();
            Root.Add(vm);
            Words.Add(vm);
            Subscribe(vm);

            System.Console.WriteLine("new word: {0}", vm);
            return vm;
        }
        /// <summary>
        /// Уничтожает созданные слова, которые не были использованы в записи.
        /// </summary>
        public void WipeUnsaved()
        {
            var toRemove = Words.Where(word => word.Unsaved).ToList();
            toRemove.ForAll((word) =>
            {
                Root.Remove(word);
                Words.Remove(word);
            });
        }

        /// <summary>
        /// Возвращает слово с указанным заголовком.
        /// </summary>
        public WordViewModel Find(string title)
        {
            return Words.Where(w => w.Name == title).SingleOrDefault();
        }

        public WordsManager(IWordRepository repo)
        {
            Contract.Requires(repo != null);

            repository = repo;

            var all = repository.GetAll().Select(s => new WordViewModel(s)).ToList();

            foreach (var item in all)
            {
                if (item.IsEnum)
                {
                    item.Add(all.Where(w => w.word.Parent == item.word));
                }
                Subscribe(item);
            }

            Root = new WordViewModel(new Word("root")) { IsNonCheckable = true };
            Root.Add(all.Where(w => w.IsRoot)); // в корне только слова верхнего уровня

            Words = new ObservableCollection<WordViewModel>(Root.Children);

            this.Subscribe((int)EventID.WordsEditingModeChanged, (e) =>
            {
                var isEditing = e.GetValue<bool>(Messages.Boolean);

                OnWordsEditingModeChanged(isEditing);
            });
        }

        private void Subscribe(WordViewModel item)
        {
            item.Editable.Committed += (s, e) =>
            {
                repository.SaveOrUpdate((e.viewModel as WordViewModel).word);
            };
        }

        private void OnWordsEditingModeChanged(bool isEditing)
        {
            Words.ForBranch((vm) =>
            {
                vm.Editable.SwitchedOn = isEditing;
                //  vm.Search.SwitchedOn = !isEditing;
            });

            UnCheckAll();
        }

        private void UnCheckAll()
        {
            Words.ForBranch((vm) => vm.IsChecked = false);
        }
    }
}