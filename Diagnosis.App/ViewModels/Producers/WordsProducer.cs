using Diagnosis.Core;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Diagnosis.App.Messaging;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class WordsProducer
    {
        private readonly IWordRepository repository;
        private readonly WordViewModel root;

        private RelayCommand _commit;
        private WordSearcher _searcher;

        public ObservableCollection<WordViewModel> Words
        {
            get;
            private set;
        }
        public WordViewModel Root { get { return root; } }


        /// <summary>
        /// Поисковик по всем словам, кроме групп, создает новые из запроса.
        /// </summary>
        public WordSearcher RootSearcher
        {
            get
            {
                return _searcher ?? (_searcher = new WordSearcher(root,
                    new SimpleSearcherSettings() { WithChecked = true, WithCreatingNew = true, AllChildren = true }));
            }
        }

        private IEnumerable<WordViewModel> DirtyWords
        {
            get
            {
                return Words.Where(s => s.Editable.IsDirty);
            }
        }

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
                                                      item.Editable.Commit();
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
                var intersect = root.AllChildren.Select(w => w.word).Intersect(s.Words);

                return root.AllChildren.Where(w => intersect.Contains(w.word));
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
            root.Add(vm);
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
                root.Remove(word);
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

        public WordsProducer(IWordRepository repo)
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

            root = new WordViewModel(new Word("root")) { IsNonCheckable = true };
            root.Add(all.Where(w => w.IsRoot)); // в корне только слова верхнего уровня

            Words = new ObservableCollection<WordViewModel>(root.Children);

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
                repository.SaveOrUpdate(e.entity as Word);
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