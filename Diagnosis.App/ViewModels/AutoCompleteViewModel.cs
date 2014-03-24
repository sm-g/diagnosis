﻿using Diagnosis.App;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System;

namespace Diagnosis.App.ViewModels
{
    public class AutoCompleteViewModel : ViewModelBase
    {
        private List<SymptomViewModel> symptoms;
        private SymptomSearch search;

        private string _fullSymptom = "";
        private bool _isSymptomCompleted;
        private int _selectedIndex;
        private ICommand _enterCommand;
        private const char SymptomSeparator = ',';

        public event EventHandler SuggestionAccepted;

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

                    try
                    {
                        MakeSuggestions();
                    }
                    catch (System.Exception e)
                    {
                        System.Windows.MessageBox.Show(e.Message);
                    }

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

        public ICommand EnterCommand
        {
            get
            {
                return _enterCommand
                    ?? (_enterCommand = new RelayCommand(
                                          () =>
                                          {
                                              if (IsSymptomCompleted)
                                              {
                                                  Clear();
                                              }
                                              else
                                              {
                                                  Accept();
                                              }
                                          }));
            }
        }

        private bool IsCompleted()
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

        public void Accept()
        {
            AddSymptom();
            FullSymptom = SymptomsChainString;

            var h = SuggestionAccepted;
            if (h != null)
            {
                h(this, new EventArgs());
            }

        }

        public void Clear()
        {
            symptoms.Clear();
            SetSearchContext(true);
            FullSymptom = "";
        }

        private void AddSymptom()
        {
            if (symptoms.Count > 0)
                symptoms[symptoms.Count - 1].AddIfNotExists(search.SelectedItem, search.AllChildren);
            search.SelectedItem.IsChecked = true;

            symptoms.Add(search.SelectedItem);
            IsSymptomCompleted = true;
        }

        private void SetSearchContext(bool symptomStarted)
        {
            var i = symptoms.Count - 1;
            if (!symptomStarted)
            {
                i--;
            }
            if (i < 0)
                search = new SymptomSearch(EntityManagers.SymptomsManager.Symptoms[0].Parent, true, true, false); // groups and cheсked, but not all children
            else
                search = new SymptomSearch(symptoms[i], true, true, false);

            if (symptomStarted)
            {
                IsSymptomCompleted = false;
            }
        }

        public void Reset()
        {
            SetSearchContext(true);
            MakeSuggestions();
        }

        public AutoCompleteViewModel()
        {
            symptoms = new List<SymptomViewModel>();

            Reset();
        }
    }
}