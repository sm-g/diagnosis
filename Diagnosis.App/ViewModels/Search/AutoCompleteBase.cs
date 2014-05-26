using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public abstract class AutoCompleteBase<T> : ViewModelBase, IAutoComplete where T : HierarchicalCheckable<T>
    {
        private int _index;
        private string _fullString = "";
        private bool _isItemCompleted;
        private ICommand _enterCommand;

        protected readonly char separator;
        protected List<T> items;
        protected ISearcher<T> searcher;

        public event EventHandler SuggestionAccepted;

        /// <summary>
        /// Запрос целиком.
        /// </summary>
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
                    Console.WriteLine("IsItemCompleted = {0}", value);
                    OnPropertyChanged(() => IsItemCompleted);
                }
            }
        }
        /// <summary>
        /// Ввод. Если элемент завершён, отмечает все элементы из коллекции, 
        /// иначе принимает выбранное предложение.
        /// </summary>
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
                                                  Reset();
                                              }
                                              else
                                              {
                                                  AcceptSuggestion();
                                              }
                                          }));
            }
        }
        /// <summary>
        /// Строка из разделённых элементов из коллекции.
        /// </summary>
        private string ItemsChain
        {
            get
            {
                var chain = items.Aggregate("", (full, s) => full += GetQueryString(s).ToLower() + separator).TrimEnd(separator);
                Console.WriteLine("Get ItemsChain: {0} ", chain);
                return chain;
            }
        }
        /// <summary>
        /// Последняя часть запроса, после разделителя.
        /// </summary>
        private string LastPart
        {
            get
            {
                Console.WriteLine("Get lastpart: {0} ", FullString.Substring(ItemsChain.Length).Trim(separator));
                return FullString.Substring(ItemsChain.Length).Trim(separator);
            }
        }

        public void Reset()
        {
            items.Clear();
            SetSearchContext(true);
            FullString = "";
        }

        /// <summary>
        /// Проверка завершенности элемента в запросе.
        /// </summary>
        /// <param name="value">Новое значение строки запроса для анализа.</param>
        private void CheckCompleted(string value)
        {
            Console.WriteLine("check completed for {0}", value);
            if (value.Length < FullString.Length)
            {
                // символ удалён

                if (IsItemCompleted)
                {
                    // удаляем последний символ
                    UncheckLast();
                    SetSearchContext(true);
                }
                else if (FullString.LastOrDefault() == separator)
                {
                    // удаляем разделитель
                    IsItemCompleted = true;
                    SetSearchContext(false);
                }
                else
                {
                    // удаляем не последний символ слова
                }
            }
            else if (value.LastOrDefault() == separator)
            {
                // добавляем разделитель

                if (IsItemCompleted)
                {
                    SetSearchContext(true);
                }
                else
                {
                    AddItem(Suggestions[SelectedIndex]);
                    SetSearchContext(true);
                }
            }
            else
            {
                // добавляем символ слова
            }

            EntityManagers.WordsManager.WipeUnsaved();
        }

        private void UncheckLast()
        {
            items.Last().IsChecked = false;
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
            Console.WriteLine("query: {0}", query);
            Suggestions = new ObservableCollection<T>(searcher.Search(query));
            OnPropertyChanged(() => Suggestions);
            SelectedIndex = 0;
        }
        /// <summary>
        /// Добавляет элемент в коллекцию.
        /// </summary>
        private void AddItem(T item)
        {
            Console.WriteLine("add {0}", item);
            BeforeAddItem(item);

            items.Add(item);
            IsItemCompleted = true;
        }
        /// <summary>
        /// Меняет поисковик, для поиска по детям последнего элемента.
        /// </summary>
        /// <param name="itemStarted"></param>
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
        /// <summary>
        /// Принимает предложение.
        /// </summary>
        private void AcceptSuggestion()
        {
            AddItem(Suggestions[SelectedIndex]);
            FullString = ItemsChain;

            var h = SuggestionAccepted;
            if (h != null)
            {
                h(this, new EventArgs());
            }
        }
        /// <summary>
        /// Отмечает элементы из коллекции.
        /// </summary>
        private void CheckItems()
        {
            foreach (var item in items)
            {
                item.IsChecked = true;
            }
        }

        protected virtual void BeforeAddItem(T item)
        {
        }

        protected abstract ISearcher<T> MakeSearch(T parent);

        protected abstract string GetQueryString(T item);

        public AutoCompleteBase(char separator = '.')
        {
            items = new List<T>();
            this.separator = separator;

            Reset();
        }
    }
}