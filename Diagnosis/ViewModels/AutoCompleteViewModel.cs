using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Collections.Generic;

namespace Diagnosis.ViewModels
{
    public class AutoCompleteViewModel : ViewModelBase
    {
        List<SymptomViewModel> symptoms;
        SearchViewModel search;

        private string _fullSymptom = "";
        private bool _isSymptomCompleted;
        int _selectedIndex;
        const char SymptomSeparator = ',';

        public string FullSymptom
        {
            get
            {
                return _fullSymptom;
            }
            set
            {
                if (_fullSymptom != value)
                {
                    CheckCompleted(value);

                    _fullSymptom = value;

                    MakeSuggestions();

                    OnPropertyChanged(() => FullSymptom);
                }
            }
        }

        public string SymptomsChainString
        {
            get
            {
                return symptoms.Aggregate("", (full, s) => full += s.Name.ToLower() + SymptomSeparator).TrimEnd(SymptomSeparator);
            }
        }

        public string LastPart
        {
            get
            {
                return FullSymptom.Substring(SymptomsChainString.Length).Trim(SymptomSeparator);
            }
        }

        bool IsCompleted()
        {
            return !IsSymptomCompleted && search.SelectedItem != null && LastPart == search.SelectedItem.Name;
        }

        /// <summary>
        /// Проверка завершенности симптома в строке
        /// </summary>
        /// <param name="value"></param>
        private void CheckCompleted(string value)
        {
            if (value.Length < FullSymptom.Length)
            {
                // символ удалён

                if (IsSymptomCompleted)
                {
                    // удаляем последний символ симптома
                    UncheckLastSymptom();
                    SetSearchContext(true);
                }
                else if (FullSymptom[FullSymptom.Length - 1] == SymptomSeparator)
                {
                    // удаляем разделитель симптомов
                    IsSymptomCompleted = true;
                    SetSearchContext(false);
                }
            }
            else if (value[value.Length - 1] == SymptomSeparator)
            {
                // добавляем разделитель

                if (IsSymptomCompleted)
                {
                    SetSearchContext(true);
                }
                else
                {
                    AddSymptom();
                    SetSearchContext(true);
                }
            }
        }

        private void UncheckLastSymptom()
        {
            symptoms.Last().IsChecked = false;
            symptoms.RemoveAt(symptoms.Count - 1);
        }

        private void MakeSuggestions()
        {
            if (!IsSymptomCompleted)
            {
                search.Query = LastPart;
            }
            else
            {
                search.Query = symptoms[symptoms.Count - 1].Name;
            }
            Suggestions = search.Results;
            OnPropertyChanged(() => Suggestions);
            SelectedIndex = 0;

        }
        public ObservableCollection<SymptomViewModel> Suggestions { get; private set; }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    search.SelectedIndex = value;
                    OnPropertyChanged(() => SelectedIndex);
                }
            }
        }

        public bool IsSymptomCompleted
        {
            get
            {
                return _isSymptomCompleted;
            }
            set
            {
                if (_isSymptomCompleted != value)
                {
                    _isSymptomCompleted = value;
                    OnPropertyChanged(() => IsSymptomCompleted);
                }
            }
        }

        public void Accept()
        {
            AddSymptom();
            FullSymptom = SymptomsChainString;
        }
        public void Clear()
        {
            symptoms.Clear();
            SetSearchContext(true);
            FullSymptom = "";
        }

        void AddSymptom()
        {
            symptoms.Add(search.SelectedItem);
            search.SelectedItem.IsChecked = true;
            IsSymptomCompleted = true;
        }

        void SetSearchContext(bool symptomStarted)
        {
            var i = symptoms.Count - 1;
            if (!symptomStarted)
            {
                i--;
            }
            if (i < 0)
                search = new SearchViewModel(DataCreator.Symptoms[0]);
            else
                search = new SearchViewModel(symptoms[i]);
            search.WithGroups = true;

            if (symptomStarted)
            {
                IsSymptomCompleted = false;
            }
        }

        public AutoCompleteViewModel()
        {
            symptoms = new List<SymptomViewModel>();

            SetSearchContext(true);
            MakeSuggestions();
        }

    }
}