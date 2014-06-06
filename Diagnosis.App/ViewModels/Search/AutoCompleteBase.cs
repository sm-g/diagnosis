using Diagnosis.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public abstract class AutoCompleteBase<T> : ViewModelBase, IAutoComplete where T : class
    {
        private int _index;
        private string _fullString = "";
        private bool _isItemCompleted;
        private ICommand _enterCommand;

        private QuerySeparator separator;
        private bool settingFullStringFromCode;

        protected ObservableCollection<T> items;
        protected ISimpleSearcher<T> searcher;
        protected SimpleSearcherSettings settings;

        public event EventHandler<AutoCompleteEventArgs> SuggestionAccepted;
        public event EventHandler InputEnded;

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(FullString.Length >= ItemsChain.Length);
        }

        public ReadOnlyObservableCollection<T> Items { get; private set; }

        public char DelimSpacer
        {
            get
            {
                return separator.Spacer;
            }
        }

        public char Delimiter
        {
            get
            {
                return separator.Delimiter;
            }
        }

        /// <summary>
        /// Завершенность слова, true, после ввода или удаления разделителя или подтверждения выбранного предположения.
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
                    OnPropertyChanged("IsItemCompleted");
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
                        if (!settingFullStringFromCode)
                        {
                            if (value.Length < FullString.Length)
                            {
                                AfterQueryShrink();
                            }
                            else
                            {
                                value = AfterQueryGrow(value);
                            }
                        }
                        _fullString = value;
                        MakeSuggestions();
                    }
                    catch (System.Exception e)
                    {
                        Console.WriteLine("FullString setter error: {0}", e.Message);
                    }

                    OnPropertyChanged("FullString");
                }
            }
        }

        /// <summary>
        /// Строка из разделённых элементов из коллекции.
        /// </summary>
        private string ItemsChain
        {
            get
            {
                var chain = items.Aggregate("", (full, s) => full += GetQueryString(s).ToLower() + separator.DelimGroup);
                if (items.Count > 0)
                {
                    chain = chain.Substring(0, chain.Length - separator.DelimGroup.Length);
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
                if (FullString.EndsWith(separator.DelimGroup))
                    delimOffset = separator.DelimGroup.Length;
                else if (FullString.EndsWith(separator.ToString()))
                    delimOffset = 1;
                else if (items.Count > 0)
                {
                    delimOffset = separator.DelimGroup.Length;
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
                    OnPropertyChanged("SelectedIndex");
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
                    if (IsItemCompleted || Suggestions.Count == 0)
                    {
                        OnInputEnded();
                    }
                    else
                    {
                        AcceptSuggestion(Suggestions[SelectedIndex]);
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
        private string AfterQueryGrow(string value)
        {
            var trimed = separator.FormatDelimiters(value);
            if (value.LastOrDefault() == separator.Delimiter)
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
                    trimed = ItemsChain + separator.DelimGroup;
                }
                IsItemCompleted = false;
                SetSearchContext();
            }
            else if (IsItemCompleted)
            {
                Console.WriteLine("дописываем символ к слову");
                // дописываем символ к слову
                RemoveLastItem();
                IsItemCompleted = false;
            }

            return trimed;
        }
        /// <summary>
        /// Проверка завершенности элемента в запросе после удаления символа.
        /// </summary>
        private void AfterQueryShrink()
        {
            if (IsItemCompleted)
            {
                Console.WriteLine("удаляем последний символ");
                // удаляем последний символ
                RemoveLastItem();
                IsItemCompleted = false;
                SetSearchContext();
            }
            else if (FullString.EndsWith(separator.DelimGroup) || FullString.LastOrDefault() == separator.Delimiter)
            {
                if (FullString.LastOrDefault() == separator.Delimiter)
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
            EntityManagers.WordsManager.WipeUnsaved();

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
            OnPropertyChanged("Suggestions");
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
        private void AcceptSuggestion(T acceptedItem)
        {
            AddItem(acceptedItem);

            settingFullStringFromCode = true;
            FullString = ItemsChain;
            settingFullStringFromCode = false;

            OnSuggestionAccepted(acceptedItem);
        }

        private void OnSuggestionAccepted(T acceptedItem)
        {
            var h = SuggestionAccepted;
            if (h != null)
            {
                h(this, new AutoCompleteEventArgs(acceptedItem));
            }
        }

        private void OnInputEnded()
        {
            var h = InputEnded;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        private void RemoveLastItem()
        {
            items.RemoveAt(items.Count - 1);
        }

        protected virtual void BeforeAddItem(T item)
        {
        }

        protected abstract ISimpleSearcher<T> MakeSearch(T parent);

        protected abstract string GetQueryString(T item);

        public AutoCompleteBase(QuerySeparator separator, SimpleSearcherSettings settings)
        {
            Contract.Requires(separator != null);
            this.settings = settings;
            this.separator = separator;

            items = new ObservableCollection<T>();
            Items = new ReadOnlyObservableCollection<T>(items);
            Suggestions = new ObservableCollection<T>();
            Reset();
        }
    }
    public class AutoCompleteEventArgs : EventArgs
    {
        public readonly object item;
        [DebuggerStepThrough]
        public AutoCompleteEventArgs(object item)
        {
            this.item = item;
        }
    }
}