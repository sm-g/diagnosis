﻿using Diagnosis.Core;
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
        protected HierarchicalSearchSettings settings;

        public event EventHandler<AutoCompleteEventArgs> SuggestionAccepted;
        public event EventHandler InputEnded;

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(FullString.Length >= ItemsChain.Length);
        }

        /// <summary>
        /// Выбранные элементы.
        /// </summary>
        public ReadOnlyObservableCollection<T> Items { get; private set; }

        /// <summary>
        /// Разделительный пробел, ставится автоматически после разделителя.
        /// </summary>
        public char DelimSpacer
        {
            get
            {
                return separator.Spacer;
            }
        }
        /// <summary>
        /// Разделитель, вводится человеком.
        /// </summary>
        public char Delimiter
        {
            get
            {
                return separator.Delimiter;
            }
        }

        /// <summary>
        /// Завершенность слова, true, после ввода разделителя или добавления элемента.
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
                    Debug.Print("AutoCompleteBase. IsItemCompleted = {0}", value);
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
                    Debug.Print("AutoCompleteBase. New Fullstring {0}", value);

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
                    if (!settingFullStringFromCode)
                    {
                        MakeSuggestions();
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
                //  Debug.Print("AutoCompleteBase. Get ItemsChain: '{0}'", chain);
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

                Debug.Print("AutoCompleteBase. Fullstring '{1}'. Get lastpart: '{0}'", last, FullString);
                return last;
            }
        }

        /// <summary>
        /// Список предлежний для заврешения ввода.
        /// </summary>
        public IList<T> Suggestions { get; private set; }

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
        /// Если элемент завершён, завершает ввод, иначе принимает выбранное предложение.
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

        /// <summary>
        /// Сбрасывает автокомплит к начальному состоянию.
        /// </summary>
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
                    Debug.Print("AutoCompleteBase. добавляем разделитель, слово было завершено");
                }
                else
                {
                    Debug.Print("AutoCompleteBase. добавляем разделитель, слово не было завершено");
                    AddItem(Suggestions[SelectedIndex]);
                    trimed = ItemsChain + separator.DelimGroup;
                }
                IsItemCompleted = false;
                SetSearchContext();
            }
            else if (IsItemCompleted)
            {
                Debug.Print("AutoCompleteBase. дописываем символ к слову");
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
                Debug.Print("AutoCompleteBase. удаляем последний символ");
                // удаляем последний символ
                RemoveLastItem();
                IsItemCompleted = false;
                SetSearchContext();
            }
            else if (FullString.EndsWith(separator.DelimGroup) || FullString.LastOrDefault() == separator.Delimiter)
            {
                if (FullString.LastOrDefault() == separator.Delimiter)
                {
                    Debug.Print("AutoCompleteBase. удаляем разделитель");
                    // удаляем разделитель
                    IsItemCompleted = true;
                    SetSearchContext();
                }
                else
                {
                    Debug.Print("AutoCompleteBase. удаляем разделительный пробел");
                    // удаляем разделительный пробел
                }
            }
            else
            {
                Debug.Print("AutoCompleteBase. удаляем не последний символ слова");
                // удаляем не последний символ слова
            }
        }

        /// <summary>
        /// Добавляет элемент в коллекцию.
        /// </summary>
        private void AddItem(T item)
        {
            Debug.Print("AutoCompleteBase. add {0}", item);
            BeforeAddItem(item);

            items.Add(item);
            IsItemCompleted = true;
        }
        private void RemoveLastItem()
        {
            items.RemoveAt(items.Count - 1);
        }

        private void MakeSuggestions()
        {
            EntityProducers.WordsProducer.WipeUnsaved();

            string query;
            if (!IsItemCompleted)
            {
                query = LastPart;
            }
            else
            {
                query = GetQueryString(items[items.Count - 1]);
            }
            Debug.Print("AutoCompleteBase. query: {0}", query);
            Suggestions = new List<T>(searcher.Search(query));
            OnPropertyChanged("Suggestions");
            SelectedIndex = 0;
        }

        /// <summary>
        /// Меняет поисковик, для поиска по последнему элементу.
        /// </summary>
        private void SetSearchContext(bool withInitItems = false)
        {
            var i = items.Count - 1;
            if (IsItemCompleted && !withInitItems)
            {
                i--;
            }

            if (i < 0)
                searcher = MakeSearch(null, Items);
            else
                searcher = MakeSearch(items[i], Items);
        }
        /// <summary>
        /// Принимает предложение.
        /// </summary>
        private void AcceptSuggestion(T acceptedItem)
        {
            AddItem(acceptedItem);

            MakeFullStringFromItems();

            OnSuggestionAccepted(acceptedItem);
        }

        /// <summary>
        /// Меняет строку запроса в соответствии с выбранными элементами без осуществления запроса.
        /// </summary>
        private void MakeFullStringFromItems()
        {
            settingFullStringFromCode = true;
            FullString = ItemsChain;
            settingFullStringFromCode = false;
        }

        protected virtual void OnSuggestionAccepted(T acceptedItem)
        {
            var h = SuggestionAccepted;
            if (h != null)
            {
                h(this, new AutoCompleteEventArgs(acceptedItem));
            }
        }

        protected virtual void OnInputEnded()
        {
            var h = InputEnded;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }
        protected virtual void BeforeAddItem(T item)
        {
        }

        protected abstract ISimpleSearcher<T> MakeSearch(T parent, IEnumerable<T> checkedItems);

        protected abstract string GetQueryString(T item);


        public AutoCompleteBase(QuerySeparator separator, HierarchicalSearchSettings settings, IEnumerable<T> initItems = null)
        {
            Contract.Requires(separator != null);
            this.settings = settings;
            this.separator = separator;

            bool withInitItems = initItems != null && initItems.Count() > 0;

            if (withInitItems)
            {
                items = new ObservableCollection<T>(initItems);
                IsItemCompleted = true;
            }
            else
            {
                items = new ObservableCollection<T>();
            }

            Items = new ReadOnlyObservableCollection<T>(items);
            Suggestions = new ObservableCollection<T>();

            SetSearchContext(withInitItems);
            MakeFullStringFromItems();
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