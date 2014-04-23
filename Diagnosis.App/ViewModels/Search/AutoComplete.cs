using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Controls;

namespace Diagnosis.App.ViewModels
{
    public class AutoComplete
    {
        private SearchWrap _selected;

        public ObservableCollection<SearchWrap> Words { get; private set; }

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
                    Console.WriteLine(value);
                }
            }
        }

        private RelayCommand _selectCommand;

        /// <summary>
        /// Gets the SelectCommand.
        /// </summary>
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
                                                      foreach (var wordVM in svm.Words)
                                                      {
                                                          wordVM.checkable.IsChecked = true;
                                                      }
                                                  }
                                              }
                                          }));
            }
        }

        public AutoCompleteFilterPredicate<object> ItemFilter
        {
            get
            {
                return (query, wrap) =>
                {
                    var vm = (wrap as SearchWrap).entity;
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
                            var words = query.Split(',')
                                .Select(q => EntityManagers.WordsManager.Find(q));

                            return words.IsSubsetOf(svm.Words);
                        }
                    }

                    return false;
                };
            }
        }

        public AutoComplete()
        {
            Words = new ObservableCollection<SearchWrap>();

            EntityManagers.WordsManager.Words.
                Select(w => new SearchWrap(w)).
                ForAll(sw => Words.Add(sw));

            EntityManagers.SymptomsManager.Symptoms.
                Select(s => new SearchWrap(s)).
                ForAll(sw => Words.Add(sw));
        }
    }
}