﻿using Diagnosis.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class WordsListViewModel : ViewModelBase
    {
        private FilterViewModel<WordViewModel> filter;

        private ICommand _add;
        private ICommand _commit;

        public event HierarchicalEventHandler<WordViewModel> NewWordAdded;

        public string Query
        {
            get
            {
                return filter.Query;
            }
            set
            {
                filter.Query = value;
            }
        }

        public ObservableCollection<WordViewModel> Words
        {
            get { return filter.Results; }
        }

        public ICommand ClearCommand
        {
            get
            {
                return filter.ClearCommand;
            }
        }

        public ICommand AddCommand
        {
            get
            {
                return _add
                   ?? (_add = new RelayCommand<WordViewModel>((current) =>
                        {
                            // убираем несохраненные слова
                            EntityProducers.WordsProducer.WipeUnsaved();
                            // на первом уровне
                            foreach (var item in Words.Where(w => w.Unsaved).ToList())
                            {
                                Words.Remove(item);
                            }

                            var newVM = EntityProducers.WordsProducer.Create(filter.Query, current);
                            // новое слово открываем для редактирования
                            newVM.Editable.SwitchedOn = true;
                            newVM.IsSelected = true;
                            newVM.Editable.IsEditorActive = true;

                            if (current != null)
                            {
                                // добавили слово родителю
                                current.IsExpanded = true;
                            }
                            else
                            {
                                // to update searcher collection to new Root.Children
                                filter.Searcher = new WordTopParentSearcher();
                                filter.Filter();
                            }
                            Subscribe(newVM);

                            OnNewWordAdded(new HierarchicalEventAgrs<WordViewModel>(newVM));
                        }));
            }
        }

        /// <summary>
        /// Количество отмеченных слов.
        /// </summary>
        public int CheckedWords
        {
            get
            {
                return EntityProducers.WordsProducer.Root.CheckedChildren;
            }
        }

        protected virtual void OnNewWordAdded(HierarchicalEventAgrs<WordViewModel> e)
        {
            var h = NewWordAdded;
            if (h != null)
            {
                h(this, e);
            }
        }

        private void UncheckBranch(WordViewModel word)
        {
            word.ForBranch((w) =>
            {
                w.IsChecked = false;
                w.IsSelected = false;
            });
        }

        private void Subscribe(WordViewModel w)
        {
            w.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "IsChecked")
                {
                    OnPropertyChanged("CheckedWords");
                }
            };
            w.Editable.Deleted += (s, e) =>
            {
                UncheckBranch(w);
                Words.Remove(w); // если на первом уровне
            };
        }

        public WordsListViewModel()
        {
            EntityProducers.WordsProducer.Root.AllChildren.ForAll((w) =>
            {
                Subscribe(w);
            });

            var searcher = new WordTopParentSearcher(); // только верхний уровень
            filter = new FilterViewModel<WordViewModel>(searcher, onRemove: UncheckBranch);
            filter.Clear();// показываем все слова
        }
    }
}