using Diagnosis.Core;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Diagnosis.App.Messaging;
using System.Diagnostics;
using System.Windows.Input;
using System;

namespace Diagnosis.ViewModels
{
    public class WordsProducer
    {
        private readonly IWordRepository repository;
        private readonly WordViewModel root;

        private WordSearcher _searcher;

        public WordViewModel Root { get { return root; } }
        /// <summary>
        /// Поисковик по всем словам, кроме групп, создает новые из запроса.
        /// </summary>
        public WordSearcher RootSearcher
        {
            get
            {
                return _searcher ?? (_searcher = new WordSearcher(root,
                    new HierarchicalSearchSettings() { WithChecked = true, WithCreatingNew = true, AllChildren = true }));
            }
        }

        public IEnumerable<WordViewModel> AllWords { get { return root.AllChildren; } }

        public IEnumerable<WordViewModel> GetSymptomWords(Symptom s)
        {
            Contract.Requires(s != null);

            return AllWords.Where(w => s.Words.Contains(w.word));
        }

        /// <summary>
        /// Создает слово и добавляет в корень или к детям указанного слова.
        /// </summary>
        public WordViewModel Create(string title, WordViewModel parent = null)
        {
            var existing = Find(title);
            if (existing != null)
                throw new ArgumentException("Word with title already exists.", "title");

            var vm = new WordViewModel(new Word(title));
            vm.Editable.MarkDirty();
            if (parent == null)
            {
                root.Add(vm);
            }
            else
            {
                parent.Add(vm);
            }
            Subscribe(vm);

            Debug.Print("new word: {0}", vm);
            return vm;
        }



        /// <summary>
        /// Уничтожает созданные слова, которые не были сохранены.
        /// </summary>
        public void WipeUnsaved()
        {
            var toRemove = AllWords.Where(word => word.Unsaved).ToList();
            toRemove.ForAll((word) =>
            {
                word.Remove();
            });
        }

        /// <summary>
        /// Возвращает слово с указанным заголовком.
        /// </summary>
        public WordViewModel Find(string title)
        {
            return AllWords.Where(w => w.Name == title).SingleOrDefault();
        }

        internal WordViewModel GetByModel(Word word)
        {
            return AllWords.Where(w => w.word == word).SingleOrDefault();
        }

        public WordsProducer(IWordRepository repo)
        {
            Contract.Requires(repo != null);
            repository = repo;

            var all = repository.GetAll().Select(s => new WordViewModel(s)).ToList();
            root = new WordViewModel(new Word("root")) { IsNonCheckable = true };
            root.Add(all);

            foreach (var item in all)
            {
                if (item.word.Parent != null)
                {
                    // если у слова есть родитель, добавляем слово к нему, и удаляем из корня
                    var parentVM = all.Where(w => w.word == item.word.Parent).SingleOrDefault();
                    parentVM.Add(item);
                    root.Remove(item);
                }
                Subscribe(item);
            }

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
                var w = e.entity as Word;
                repository.SaveOrUpdate(w);
                GetByModel(w).RefreshProperties();
            };
            item.Editable.Reverted += (s, e) =>
            {
                var w = e.entity as Word;
                repository.Refresh(w);
                GetByModel(w).RefreshProperties();
            };
            item.Editable.Deleted += (s, e) =>
            {
                var w = e.entity as Word;
                repository.Remove(w);
                var vm = GetByModel(w);
                vm.Remove();
            };
            item.ChildrenChanged += (s, e) =>
            {
                // слово с детьми нельзя удалять
                SetDeletable(item);
            };
        }

        private void OnWordsEditingModeChanged(bool isEditing)
        {
            AllWords.ForAll((vm) =>
            {
                SetDeletable(vm);
            });

            UnCheckAll();
        }

        private static void SetDeletable(WordViewModel w)
        {
            // нельзя удалить слова, которые есть в каком-нибудь симптоме
            // или слова с детьми
            // TODO в симптоме, связанном с записью
            if (w.IsParent || EntityProducers.SymptomsProducer.GetSymptomsWithWords(w.ToEnumerable()).Count() > 0)
            {
                w.Editable.CanBeDeleted = false;
            }
            else
            {
                w.Editable.CanBeDeleted = true;
            }
        }

        public void CheckThese(IEnumerable<WordViewModel> words)
        {
            UnCheckAll();

            foreach (var item in words)
            {
                item.IsChecked = true;
            }
        }

        private void UnCheckAll()
        {
            AllWords.ForAll((vm) => vm.IsChecked = false);
        }
    }
}