using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using Diagnosis.Core;

namespace Diagnosis.App.ViewModels
{
    public class AutoCompleteBoxViewModel : ViewModelBase
    {
        readonly QuerySeparator separator;

        private SearchWrap _selected;
        private RelayCommand _selectCommand;
        private string _query;

        public char DelimSpacer
        {
            get
            {
                return separator.Spacer;
            }
        }

        public string Query
        {
            get
            {
                return _query;
            }
            set
            {
                Console.WriteLine("Query: {0}", value);

                var trimed = separator.FormatDelimiters(value);
                if (_query != trimed)
                {
                    MakeSuggestions(separator.RemoveSpacers(trimed));

                    _query = separator.RestoreLastDelimGroup(trimed);

                    OnPropertyChanged("Query");
                }
                else
                {
                    Console.WriteLine("same query");
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
                                              var svm = Selected.entity as SymptomViewModel;
                                              if (svm != null)
                                              {
                                                  svm.Words.ForAll(w => w.IsChecked = true);
                                              }
                                          }));
            }
        }

        private void MakeSuggestions(string query)
        {
            EntityManagers.WordsManager.WipeUnsaved();
            EntityManagers.SymptomsManager.WipeUnsaved();

            var symptoms = new List<SymptomViewModel>();
            foreach (var line in StringSequencePartition.Part(query, separator.Delimiter))
            {
                Console.Write("line of partition: ");
                foreach (var word in line)
                {
                    Console.Write("{0}, ", word);
                }
                Console.WriteLine();

                // группы из слов, начинающихся на слово из разбиения
                var wordGroups = line.Select(word => EntityManagers.WordsManager.RootSearcher.Search(word).ToList()).ToList();

                // все комбинации слов для создания симптомов
                var wordsForSymptoms = Combinator<WordViewModel>.Combine(wordGroups);

                foreach (var words in wordsForSymptoms)
                {
                    symptoms.Add(EntityManagers.SymptomsManager.Create(words));
                }
            }
            Results = new ObservableCollection<SearchWrap>(symptoms.Select(s => new SearchWrap(s)));
            OnPropertyChanged("Results");
        }

        public AutoCompleteBoxViewModel(QuerySeparator separator)
        {
            Contract.Requires(separator != null);

            this.separator = separator;
            Results = new ObservableCollection<SearchWrap>();
        }
    }
}