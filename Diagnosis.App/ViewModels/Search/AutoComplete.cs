using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class AutoComplete : ViewModelBase
    {
        private SearchWrap _selected;
        private RelayCommand _selectCommand;


        private string _query;
        public string Query
        {
            get
            {
                return _query;
            }
            set
            {
                Console.WriteLine("Query: {0}", value);
                var trimed = value.Trim();
                if (_query != trimed)
                {
                    _query = trimed;

                    List<SymptomViewModel> results = new List<SymptomViewModel>();
                    EntityManagers.WordsManager.WipeUnsaved();
                    EntityManagers.SymptomsManager.WipeUnsaved();

                    var symptoms = new List<SymptomViewModel>();
                    foreach (var line in StringSequencePartition.Part(_query))
                    {
                        Console.Write("line of partition: ");
                        foreach (var word in line)
                        {
                            Console.Write("{0}, ", word);
                        }
                        Console.WriteLine();

                        // группы из слов, начинающихся на слово из разбиения
                        var wordGroups = line.Select(word => EntityManagers.WordsManager.Searcher.Search(word).ToList()).ToList();

                        // все комбинации слов для создания симптомов
                        var wordsForSymptoms = Combinator<WordViewModel>.Combine(wordGroups);

                        foreach (var words in wordsForSymptoms)
                        {
                            symptoms.Add(EntityManagers.SymptomsManager.Create(words));
                        }
                    }
                    Results = new ObservableCollection<SearchWrap>(symptoms.Select(s => new SearchWrap(s)));
                    OnPropertyChanged(() => Results);
                }
            }
        }

        public ObservableCollection<SearchWrap> Results { get; private set; }

        public SearchWrap Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                }
            }
        }

        public ICommand SelectCommand
        {
            get
            {
                return _selectCommand ?? (_selectCommand = new RelayCommand(() =>
                                          {
                                              var wvm = Selected.entity as WordViewModel;
                                              if (wvm != null)
                                              {
                                                  wvm.checkable.IsChecked = true;
                                              }
                                              else
                                              {
                                                  var svm = Selected.entity as SymptomViewModel;
                                                  if (svm != null)
                                                  {
                                                      svm.Words.ForAll(w => w.checkable.IsChecked = true);
                                                  }
                                              }
                                          }));
            }
        }

        public AutoCompleteFilterPredicate<object> ItemFilter
        {
            get
            {
                return new AutoCompleteFilterPredicate<object>((query, obj) => FilterItem(query, obj as SearchWrap));
            }
        }

        public static bool FilterItem(string query, SearchWrap wrap)
        {
            var vm = wrap.entity;
            var wvm = vm as WordViewModel;
            if (wvm != null)
            {
                return wvm.Name.StartsWith(query, StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                var svm = vm as SymptomViewModel;
                if (svm != null)
                {
                    foreach (var line in StringSequencePartition.Part(query))
                    {
                        bool result = false;

                        // VM слов из разбиения
                        var words = line.Select(word => EntityManagers.WordsManager.Find(word)).ToList();

                        // в запросе может быть часть слова из базы или слово, которого нет в базе
                        // ищем слова или части слов, которых нет в симптоме

                        for (int i = 0; i < line.Count; i++)
                        {
                            if (words[i] == null)
                            {
                                // ложь, если в симптоме нет ни одного слова, начинающегося на часть запроса (группу в линии)
                                result = svm.Words.Any(w => w.Name.StartsWith(line[i], StringComparison.InvariantCultureIgnoreCase));
                            }
                            else
                            {
                                // ложь, если в симптоме нет некоторого слова из группы
                                result = svm.Words.Contains(words[i]);
                            }
                            if (!result)
                                break;
                        }

                        if (result)
                            return true;
                    }
                }
            }

            return false;
        }

        public AutoComplete()
        {
            Results = new ObservableCollection<SearchWrap>();
        }
    }
}