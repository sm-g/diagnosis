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
        private HierarchicalSearch<T> search;

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
                                                  Clear();
                                              }
                                              else
                                              {
                                                  Accept();
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
            if (!IsItemCompleted)
            {
                search.Query = LastPart;
            }
            else
            {
                search.Query = GetQueryString(items[items.Count - 1]);
            }
            Suggestions = new ObservableCollection<T>(search.Results);
            OnPropertyChanged(() => Suggestions);
            search.SelectedIndex = 0;
        }

        private void AddItem()
        {
            if (items.Count > 0)
                items[items.Count - 1].AddIfNotExists(search.SelectedItem, search.AllChildren);
            search.SelectedItem.checkable.IsChecked = true;

            items.Add(search.SelectedItem);
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
                search = MakeSearch(null);
            else
                search = MakeSearch(items[i]);

            if (itemStarted)
            {
                IsItemCompleted = false;
            }
        }

        private void Accept()
        {
            AddItem();
            FullString = ItemsChain;

            var h = SuggestionAccepted;
            if (h != null)
            {
                h(this, new EventArgs());
            }
        }

        private void Clear()
        {
            items.Clear();
            SetSearchContext(true);
            FullString = "";
        }

        protected abstract HierarchicalSearch<T> MakeSearch(T parent);

        protected abstract string GetQueryString(T item);

        public AutoCompleteBase()
        {
            items = new List<T>();

            Reset();
        }
    }
}