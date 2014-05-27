using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Diagnostics.Contracts;

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

        string delimGroup;
        string separatorEsc;
        public char DelimSpacer { get { return ' '; } }

        public event EventHandler SuggestionAccepted;

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(FullString.Length >= ItemsChain.Length);
        }

        /// <summary>
        /// Завершенность слова, true, после ввода или удаления разделителя.
        /// </summary>
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
                    Console.WriteLine();
                    try
                    {
                        if (value.Length < FullString.Length)
                        {
                            CheckAfterDeletion();
                        }
                        else
                        {
                            value = CheckAfterAdding(value);
                        }

                        _fullString = value;

                        MakeSuggestions();
                    }
                    catch (System.Exception e)
                    {
                        Console.WriteLine("FullString setter error: {0}", e.Message);
                    }

                    OnPropertyChanged(() => FullString);
                }
            }
        }
        /// <summary>
        /// Удаляет лишние разделительные символы.
        /// Например, "абв..  где.  ." → "абс. где. "
        /// </summary>
        private string TrimExcessDelimeters(string value)
        {
            Console.WriteLine("before trim '{0}'", value);

            // оставляем по одному пробелу
            var trimed = Regex.Replace(value, @"\s+", DelimSpacer.ToString());
            // повторные группы разделительных символов заменяем на одну группу
            trimed = Regex.Replace(trimed, @"[\s" + separatorEsc + "]+", delimGroup);

            Console.WriteLine("after trim '{0}'", trimed);
            return trimed;
        }
        /// <summary>
        /// Строка из разделённых элементов из коллекции.
        /// </summary>
        private string ItemsChain
        {
            get
            {
                var chain = items.Aggregate("", (full, s) => full += GetQueryString(s).ToLower() + delimGroup);
                if (items.Count > 0)
                {
                    chain = chain.Substring(0, chain.Length - delimGroup.Length);
                }
                Console.WriteLine("Get ItemsChain: '{0}'", chain);
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
                string last = "";

                int delimOffset = 0;
                if (FullString.EndsWith(delimGroup))
                    delimOffset = delimGroup.Length;
                else if (FullString.EndsWith(separator.ToString()))
                    delimOffset = 1;
                else if (items.Count > 0)
                {
                    delimOffset = delimGroup.Length;
                }

                if (ItemsChain.Length + delimOffset < FullString.Length)
                {
                    last = FullString.Substring(ItemsChain.Length + delimOffset);
                }

                Console.WriteLine("fullstring '{1}'. Get lastpart: '{0}'", last, FullString);
                return last;
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
        /// <summary>
        /// Ввод. Если элемент завершён, отмечает все элементы из коллекции, 
        /// иначе принимает выбранное предложение.
        /// </summary>
        public ICommand EnterCommand
        {
            get
            {
                return _enterCommand ?? (_enterCommand = new RelayCommand(() =>
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

        public void Reset()
        {
            items.Clear();
            IsItemCompleted = false;
            SetSearchContext();
            FullString = "";
        }

        /// <summary>
        /// Проверка завершенности элемента в запросе после добавления символа.
        /// </summary>
        /// <param name="value">Новое значение строки запроса для анализа.</param>
        private string CheckAfterAdding(string value)
        {
            var trimed = TrimExcessDelimeters(value);
            if (value.LastOrDefault() == separator)
            {
                // добавляем разделитель
                if (IsItemCompleted)
                {
                    Console.WriteLine("добавляем разделитель, слово было завершено");
                }
                else
                {
                    Console.WriteLine("добавляем разделитель, слово не было завершено");
                    AddItem(Suggestions[SelectedIndex]);
                    trimed = ItemsChain + delimGroup;
                }
                IsItemCompleted = false;
                SetSearchContext();
            }
            else if (IsItemCompleted)
            {
                Console.WriteLine("дописываем символ к слову");
                // дописываем символ к слову
                UncheckLastItem();
                IsItemCompleted = false;
            }

            return trimed;
        }
        /// <summary>
        /// Проверка завершенности элемента в запросе после удаления символа.
        /// </summary>
        private void CheckAfterDeletion()
        {
            if (IsItemCompleted)
            {
                Console.WriteLine("удаляем последний символ");
                // удаляем последний символ
                UncheckLastItem();
                IsItemCompleted = false;
                SetSearchContext();
            }
            else if (FullString.EndsWith(delimGroup) || FullString.LastOrDefault() == separator)
            {
                if (FullString.LastOrDefault() == separator)
                {
                    Console.WriteLine("удаляем разделитель");
                    // удаляем разделитель
                    IsItemCompleted = true;
                    SetSearchContext();
                }
                else
                {
                    Console.WriteLine("удаляем разделительный пробел");
                    // удаляем разделительный пробел
                }
            }
            else
            {
                Console.WriteLine("удаляем не последний символ слова");
                // удаляем не последний символ слова
            }
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
        /// Меняет поисковик, для поиска по последнему элементу.
        /// </summary>
        private void SetSearchContext()
        {
            var i = items.Count - 1;
            if (IsItemCompleted)
            {
                i--;
            }

            if (i < 0)
                searcher = MakeSearch(null);
            else
                searcher = MakeSearch(items[i]);
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

        private void UncheckLastItem()
        {
            items.Last().IsChecked = false;
            items.RemoveAt(items.Count - 1);
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
            delimGroup = separator.ToString() + DelimSpacer.ToString();
            separatorEsc = Regex.Escape(separator.ToString());

            Reset();
        }
    }
}