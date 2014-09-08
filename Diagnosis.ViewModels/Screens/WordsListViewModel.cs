using Diagnosis.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using EventAggregator;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using Diagnosis.Data.Repositories;
using Diagnosis.Data;
using NHibernate;
using Diagnosis.Data.Queries;

namespace Diagnosis.ViewModels
{
    public class WordsListViewModel : SessionVMBase
    {
        private NewFilterViewModel<Word> _filter;

        public NewFilterViewModel<Word> Filter
        {
            get { return _filter; }
        }

        public ObservableCollection<WordViewModel> Words
        {
            get;
            private set;
        }

        public RelayCommand<WordViewModel> AddCommand
        {
            get
            {
                return new RelayCommand<WordViewModel>((current) =>
                        {
                            // убираем несохраненные слова



                            var newW = new Word(_filter.Query);
                            newW.Parent = current != null ? current.word : null;
                            var newVM = new WordViewModel(newW);

                            // новое слово открываем для редактирования
                            newVM.IsSelected = true;
                            // newVM.Editable.IsEditorActive = true;

                            if (current != null)
                            {
                                // добавили слово родителю
                                current.IsExpanded = true;
                            }
                            else
                            {
                                // to update searcher collection to new Root.Children
                                //filter.Searcher = new WordTopParentSearcher();
                                // filter.Filter();
                            }
                            Subscribe(newVM);
                        });
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
                                .AsParams(MessageKeys.Words));
                        }, () => CheckedWordsNumber > 0);
            }
        }

        /// <summary>
        /// Количество отмеченных слов.
        /// </summary>
        public int CheckedWordsNumber
        {
            get
            {
                return Words.Count(w => w.IsChecked);
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

        private void Subscribe(WordViewModel vm)
        {
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "IsChecked")
                {
                    OnPropertyChanged("CheckedWords");
                }
            };
            //w.Editable.Deleted += (s, e) =>
            //{
            //    UncheckBranch(w);
            //    Words.Remove(w); // если на первом уровне
            //};
            vm.Editable.Committed += (s, e) =>
            {
                var w = e.entity as Word;
                Session.SaveOrUpdate(w);
            };
            vm.Editable.Reverted += (s, e) =>
            {
                var w = e.entity as Word;
                Session.Refresh(w);
            };
            vm.Editable.Deleted += (s, e) =>
            {
                var w = e.entity as Word;
                Session.Delete(w);
            };
            vm.ChildrenChanged += (s, e) =>
            {
                // слово с детьми нельзя удалять
                SetDeletable(vm);
            };
        }

        public WordsListViewModel()
        {
            Words = new ObservableCollection<WordViewModel>();

            _filter = new NewFilterViewModel<Word>(WordQuery.StartingWith(Session));


            _filter.Results.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    foreach (Word item in e.OldItems)
                    {
                        var deleted = Words.Where(w => w.word == item).ToList();
                        deleted.ForEach((w) => Words.Remove(w));
                    }
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    foreach (Word item in e.NewItems)
                    {
                        var newVM = new WordViewModel(item);
                        Words.Add(newVM);
                    }
            };

            _filter.Clear();// показываем все слова
        }

        private static void SetDeletable(WordViewModel w)
        {
            // нельзя удалить 
            //  TODO слова, которые есть в каком-нибудь симптоме, связанном с записью. 
            // слова с детьми
            if (!w.IsTerminal)
            {
                w.Editable.CanBeDeleted = false;
            }
            else
            {
                w.Editable.CanBeDeleted = true;
            }
        }
    }
}