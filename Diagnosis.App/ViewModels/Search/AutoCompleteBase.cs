using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public abstract class AutoCompleteBase<T> : ViewModelBase, IAutoComplete where T : HierarchicalCheckable<T>
    {
        private List<T> items;
        private ISearcher<T> searcher;
        private int _index;

        private string _fullString = "";
        private bool _isItemCompleted;
        private ICommand _enterCommand;
        private const char Separator = ',';

        public event EventHandler SuggestionAccepted;

        public string FullString
        {
            get
            {
                return _fullString;
            }
            set
            {
                if (_fullString != value)
                {
                    CheckCompleted(value);

                    _fullString = value;

                    try
                    {
                        MakeSuggestions();
                    }
                    catch (System.Exception e)
                    {
                        System.Windows.MessageBox.Show(e.Message);
                    }

                    OnPropertyChanged(() => FullString);
                }
            }
        }

        public ObservableCollection<T> Suggestions { get; private set; }

        public int SelectedIndex
        {
            get
            {
                return _index;
            }
            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnPropertyChanged(() => SelectedIndex);
                }
            }
        }

        public bool IsItemCompleted
        {
            get
            {
                return _isItemCompleted;
            }
            private set
            {
                if (_isItemCompleted != value)
                {
                    _isItemCompleted = value;
                    OnPropertyChanged(() => IsItemCompleted);
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
                                              if (IsItemCompleted)
                                              {
                                                  CheckItems();
                                              }
                                              else
                                              {
                                                  AcceptSuggestion();
                                              }
                                          }));
            }
        }

        private string ItemsChain
        {
            get
            {
                return items.Aggregate("", (full, s) => full += GetQueryString(s).ToLower() + Separator).TrimEnd(Separator);
            }
        }

        private string LastPart
        {
            get
            {
                return FullString.Substring(ItemsChain.Length).Trim(Separator);
            }
        }

        public void Reset()
        {
            SetSearchContext(true);
            MakeSuggestions();
        }

        /// <summary>
        /// Проверка завершенности в строке
        /// </summary>
        /// <param name="value"></param>
        private void CheckCompleted(string value)
        {
            if (value.Length < FullString.Length)
            {
                // символ удалён

                if (IsItemCompleted)
                {
                    // удаляем последний символ
                    UncheckLast();
                    SetSearchContext(true);
                }
                else if (FullString[FullString.Length - 1] == Separator)
                {
                    // удаляем разделитель
                    IsItemCompleted = true;
                    SetSearchContext(false);
                }
            }
            else if (value[value.Length - 1] == Separator)
            {
                // добавляем разделитель

                if (IsItemCompleted)
                {
                    SetSearchContext(true);
                }
                else
                {
                    AddItem();
                    SetSearchContext(true);
                }
            }
        }

        private void UncheckLast()
        {
            items.Last().checkable.IsChecked = false;
            items.RemoveAt(items.Count - 1);
        }

        private void MakeSuggestions()
        {
            string query;
            if (!IsItemCompleted)
            {
                query = LastPart;
            }
            else
            {
                query = GetQueryString(items[items.Count - 1]);
            }
            Suggestions = new ObservableCollection<T>(searcher.Search(query));
            OnPropertyChanged(() => Suggestions);
            SelectedIndex = 0;
        }

        private void AddItem()
        {
            if (items.Count > 0)
                items[items.Count - 1].AddIfNotExists(Suggestions[SelectedIndex], searcher.AllChildren);

            items.Add(Suggestions[SelectedIndex]);
            IsItemCompleted = true;
        }

        private void SetSearchContext(bool itemStarted)
        {
            var i = items.Count - 1;
            if (!itemStarted)
            {
                i--;
            }

            if (i < 0)
                searcher = MakeSearch(null);
            else
                searcher = MakeSearch(items[i]);

            if (itemStarted)
            {
                IsItemCompleted = false;
            }
        }

        private void AcceptSuggestion()
        {
            AddItem();
            FullString = ItemsChain;

            var h = SuggestionAccepted;
            if (h != null)
            {
                h(this, new EventArgs());
            }
        }

        private void CheckItems()
        {
            foreach (var item in items)
            {
                item.checkable.IsChecked = true;
            }
            items.Clear();
            SetSearchContext(true);
            FullString = "";
        }

        protected abstract ISearcher<T> MakeSearch(T parent);

        protected abstract string GetQueryString(T item);

        public AutoCompleteBase()
        {
            items = new List<T>();

            Reset();
        }
    }
}