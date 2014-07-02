using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Diagnosis.Core;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class WordsListViewModel : ViewModelBase
    {
        private readonly FilterViewModel<WordViewModel> filter;

        private ICommand _add;
        private ICommand _commit;
        private ObservableCollection<WordViewModel> _words;

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
                ShowFilteredWords(filter.Results);
            }
        }

        public ObservableCollection<WordViewModel> Words
        {
            get { return _words; }
            private set
            {
                _words = value;
                OnPropertyChanged("Words");
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return filter.ClearCommand;
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
                            // сохраняем выбранные слова
                            foreach (var item in Words.Where(w => w.IsChecked))
                            {
                                item.Editable.Commit();
                            }
                        },
                        () => Words.Where(w => w.IsChecked).Any(w => w.Editable.IsDirty)));
            }
        }

        public ICommand AddCommand
        {
            get
            {
                return _add
                   ?? (_add = new RelayCommand<WordViewModel>((current) =>
                        {
                            EntityProducers.WordsProducer.WipeUnsaved();

                            // убираем несохраненные слова на первом уровне
                            var unsaved = Words.Where(w => w.Unsaved).ToList();
                            foreach (var item in unsaved)
                            {
                                Words.Remove(item);
                            }

                            var newVM = EntityProducers.WordsProducer.Create("", current);
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
                                AddToTree(newVM);
                            }

                            OnNewWordAdded(new HierarchicalEventAgrs<WordViewModel>(newVM));
                        }));
            }
        }

        public int CheckedWords
        {
            get
            {
                return Words.Where(w => w.IsChecked).Count();
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

        private void ShowFilteredWords(IEnumerable<WordViewModel> words)
        {
            Words.Except(words).ToList().ForAll((w) => Words.Remove(w)); // remove unwanted
            foreach (var w in words.Except(Words).ToList())
            {
                AddToTree(w);
            }
        }

        private void AddToTree(WordViewModel w)
        {
            Words.Add(w);
            w.Editable.Deleted += (s, e) =>
            {
                // убираем удаленные слова на первом уровне
                Words.Remove(w);
            };
            w.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "IsChecked")
                {
                    OnPropertyChanged("CheckedWords");
                }
            };
        }

        public WordsListViewModel(WordViewModel root)
        {
            Words = new ObservableCollection<WordViewModel>();
            var searcher = new WordTopParentSearcher();
            filter = new FilterViewModel<WordViewModel>(searcher);
            filter.Clear(); // показываем все слова

            ShowFilteredWords(filter.Results);
        }
    }
}