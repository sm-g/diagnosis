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

        public ObservableCollection<WordViewModel> Words
        {
            get;
            private set;
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
                var intersect = Words[0].Parent.AllChildren.Select(w => w.word).Intersect(s.Words);

                return Words[0].Parent.AllChildren.Where(w => intersect.Contains(w.word));
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

        public WordViewModel Create(string title)
        {
            var vm = new WordViewModel(new Word(title));
            vm.Editable.MarkDirty();
            Subscribe(vm);
            return vm;
        }

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
                Subscribe(item);
            }

            var root = new WordViewModel("root") { IsNonCheckable = true };
            root.Add(all);
            root.Initialize();

            Words = new ObservableCollection<WordViewModel>(root.Children);

            this.Subscribe((int)EventID.WordsEditingModeChanged, (e) =>
            {
                var isEditing = e.GetValue<bool>(Messages.Boolean);

                OnDirectoryEditingModeChanged(isEditing);
            });
        }

        private void Subscribe(WordViewModel item)
        {
            item.Editable.Committed += (s, e) =>
            {
                repository.SaveOrUpdate((e.viewModel as WordViewModel).word);
            };
        }

        private void OnDirectoryEditingModeChanged(bool isEditing)
        {
            Words.ForAll((vm) =>
            {
                vm.Editable.SwitchedOn = isEditing;
                vm.Search.SwitchedOn = !isEditing;
            });

            UnCheckAll();
        }

        private void UnCheckAll()
        {
            Words.ForAll((vm) => vm.IsChecked = false);
        }
    }
}