using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class AutoComplete
    {
        private SearchWrap _selected;
        private RelayCommand _selectCommand;

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

        public AutoCompleteFilterPredicate<string> TextFilter
        {
            get
            {
                return new AutoCompleteFilterPredicate<string>((query, value) => FilterText(query, value));
            }
        }

        public static bool FilterText(string query, string value)
        {
            return true;
        }

        public static bool FilterItem(string query, SearchWrap wrap)
        {
            return true;
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
                        var words = line.Select(word => EntityManagers.WordsManager.Find(word)).ToList();

                        if (words.Any(w => w == null))
                        {
                            // в запросе есть часть слова из базы или слово, которого нет в базе
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
                        }
                        else
                        {
                            // все слова запроса есть в базе
                            // симптом подходит, если слова запроса — подмножество слов симптома
                            result = words.IsSubsetOf(svm.Words);
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

            EntityManagers.SymptomsManager.Symptoms.
               Select(s => new SearchWrap(s)).
               ForAll(sw => Results.Add(sw));

            foreach (var word in EntityManagers.WordsManager.Words)
            {
                // без слов, для которых уже есть симптом только с этим словом
                if (!EntityManagers.SymptomsManager.Symptoms.Any(s => s.Words.Contains(word)))
                {
                    Results.Add(new SearchWrap(word));
                }
            }
        }
    }
}