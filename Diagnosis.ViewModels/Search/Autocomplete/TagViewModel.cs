﻿using Diagnosis.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Diagnosis.Common;
using System.Windows;
using GongSolutions.Wpf.DragDrop;

namespace Diagnosis.ViewModels.Search.Autocomplete
{
    [Flags]
    public enum Signalizations
    {
        None,
        /// <summary>
        /// Тег содержит новое слово
        /// </summary>
        NewWord,
        /// <summary>
        /// Частичное измерение с неправильной единицей.
        /// </summary>
        PartialMeasure = 2,
        /// <summary>
        /// Некорректный тег (новый без заготовки).
        /// </summary>
        Forbidden = 4
    }
    /// <summary>
    /// Типы заготовок в теге.
    /// </summary>
    public enum BlankType
    {
        None,
        /// <summary>
        /// Строка-запрос
        /// </summary>
        Query,
        Comment,
        Word,
        Measure,
        Icd
    }

    /// <summary>
    /// Состояния тега.
    /// </summary>
    public enum State
    {
        /// <summary>
        /// Новый (пустой) - начальное состояние
        /// </summary>
        Init,
        /// <summary>
        /// Редактируется
        /// </summary>
        Typing,
        /// <summary>
        /// Завершен
        /// </summary>
        Completed
    }

    public class TagViewModel : ViewModelBase, IDropTarget
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TagViewModel));
        readonly AutocompleteViewModel autocomplete;
        private object _blank;
        private bool _focused;
        private string _query;
        private State _state;
        private bool _isDeleteOnly;
        private bool _isLast;
        private bool _listItemFocused;
        private bool _selected;
        private bool _draggable;
        private Signalizations? _signal;

        /// <summary>
        /// Создает пустой тег.
        /// </summary>
        public TagViewModel(AutocompleteViewModel parent)
        {
            Contract.Requires(parent != null);
            Contract.Ensures(State == State.Init);

            this.autocomplete = parent;
            Reset();
        }

        /// <summary>
        /// Создает тег с запросом.
        /// </summary>
        public TagViewModel(AutocompleteViewModel parent, string query)
        {
            Contract.Requires(parent != null);
            Contract.Ensures(State == State.Typing);

            this.autocomplete = parent;
            Query = query;
        }


        /// <summary>
        /// Создает тег с сущностями.
        /// </summary>
        public TagViewModel(AutocompleteViewModel parent, IHrItemObject item)
        {
            Contract.Requires(parent != null);
            Contract.Requires(item != null);
            Contract.Ensures(State == State.Completed);

            this.autocomplete = parent;

            Blank = item;
            Entity = item;
        }

        public event EventHandler Deleted;
        public event EventHandler<BlankTypeEventArgs> Converting;

        /// <summary>
        /// Текстовое представление.
        /// </summary>
        public string Query
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _query;
            }
            set
            {
                if (_query != value)
                {
                    // logger.DebugFormat("query = {0}", value);
                    Contract.Assume(!IsDeleteOnly);

                    State = State.Typing;

                    // show drag when type in last
                    if (IsLast && !value.IsNullOrEmpty())
                        IsDraggable = true;

                    _query = value ?? string.Empty;
                    Entity = null;
                    OnPropertyChanged("Query");
                }
            }
        }

        public State State
        {
            get { return _state; }
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        /// <summary>
        /// Сущность, созданная из тега. При изменении запроса или бланка сбрасывается.
        /// Копируется.
        /// </summary>
        public IHrItemObject Entity { get; internal set; }

        /// <summary>
        /// Заготовка, из которой получаются сущности.
        /// То, что оказалось введенным - найденное слово, текст запроса, МКБ, измерение или ничего.
        /// </summary>
        public object Blank
        {
            get
            {
                return _blank;
            }
            set
            {
                if (_blank != value)
                {
                    // logger.DebugFormat("blank = {0} ({1})", value, GetBlankType(value));

                    _blank = value;
                    OnPropertyChanged("Blank");
                    OnPropertyChanged("BlankType");
                    OnPropertyChanged(() => Focusable);
                }
                if (value != null)
                {
                    Query = value.ToString();
                }
                State = State.Completed;
                Signalization = null;
            }
        }

        /// <summary>
        /// Тип заготовки.
        /// </summary>
        public BlankType BlankType
        {
            get
            {
                return GetBlankType(Blank);
            }
            set { } // for binding
        }
        #region AutocompleteRelated


        public RelayCommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    autocomplete.EditCommand.Execute(null);
                });
            }
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!IsLast && !autocomplete.SingleTag)
                    {
                        OnDeleted();
                    }
                    else
                    {
                        Reset();
                    }
                });
            }
        }

        public RelayCommand SendToSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    autocomplete.SendToSearchCommand.Execute(this);
                },
                () => autocomplete.WithSendToSearch);
            }
        }
        public RelayCommand<BlankType> ConvertToCommand
        {
            get
            {
                return new RelayCommand<BlankType>((t) =>
                {
                    OnConverting(t);
                },
                (t) => autocomplete.WithConvert && t != BlankType && State != State.Init && !Query.IsNullOrEmpty() && !IsDeleteOnly);
            }
        }

        public RelayCommand AddLeftCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    autocomplete.AddTag(this, true);
                }, () => !autocomplete.SingleTag);
            }
        }
        public RelayCommand AddRightCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    autocomplete.AddTag(this, false);
                }, () => !IsLast && !autocomplete.SingleTag);
            }
        }


        /// <summary>
        /// Тег не редактируется, можно только удалить.
        /// </summary>
        public bool IsDeleteOnly
        {
            get
            {
                return _isDeleteOnly;
            }
            private set
            {
                if (_isDeleteOnly != value)
                {
                    _isDeleteOnly = value;
                    OnPropertyChanged("IsDeleteOnly");
                }
            }
        }

        /// <summary>
        /// Специальный последний тег для набора текста по умолчанию. 
        /// Не удаляется. Не копируется.
        /// Всегда есть, кроме случая, когда в автокомплите единствевнный тег.
        /// </summary>
        public bool IsLast
        {
            get { return _isLast; }
            set
            {
                if (_isLast != value)
                {
                    _isLast = value;

                    // Last tag is not draggable before it has query
                    if (value)
                        IsDraggable = false;

                    OnPropertyChanged(() => IsLast);
                }
            }
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            autocomplete.DropHandler.DragOver(dropInfo);
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            autocomplete.DropHandler.Drop(dropInfo);
        }

        #endregion


        #region ViewRelated
        public Signalizations? Signalization
        {
            get
            {
                return _signal;
            }
            set
            {
                if (_signal != value)
                {
                    _signal = value;
                    //  logger.InfoFormat("{0} signals", this);
                    OnPropertyChanged(() => Signalization);
                }
            }
        }
        /// <summary>
        /// Редактируется (слово или коммент)
        /// </summary>
        public bool IsTextBoxFocused
        {
            get
            {
                return _focused;
            }
            set
            {
                if (_focused != value)
                {
                    _focused = value;
                    //logger.DebugFormat("{0} focusedTxt = {1}, focusedItem = {2}", this, value, _listItemFocused);
                    OnPropertyChanged("IsTextBoxFocused");
                }
            }
        }

        public bool Focusable
        {
            get
            {
                return BlankType != BlankType.Icd   // редактируются через отдельный редактор
                    && BlankType != BlankType.Measure;
            }
        }
        /// <summary>
        /// Тег выделен
        /// </summary>
        public bool IsListItemFocused
        {
            get
            {
                return _listItemFocused;

            }
            set
            {
                if (_listItemFocused != value)
                {
                    _listItemFocused = value;
                    //logger.DebugFormat("{0} focusedItem = {1}, focusedTxt = {2}", this, value, _focused);

                    OnPropertyChanged(() => IsListItemFocused);
                }
            }
        }
        public bool IsSelected
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
                    // logger.DebugFormat("{0} selected {1}", this, value);
                    OnPropertyChanged(() => IsSelected);
                }
            }
        }
        public bool IsDraggable
        {
            get
            {
                return _draggable;
            }
            set
            {
                if (_draggable != value)
                {
                    _draggable = value;
                    OnPropertyChanged(() => IsDraggable);
                }
            }
        }
        /// <summary>
        /// Radio group name
        /// </summary>
        public string Hash { get { return GetHashCode().ToString(); } }

        /// <summary>
        /// Switch focus between textbox and listitem for SelectedTag
        /// </summary>
        public void SwitchEdit()
        {
            if (IsTextBoxFocused)
            {
                IsListItemFocused = true;
            }
            else
            {
                IsTextBoxFocused = true;
            }
        }
        #endregion

        public void Validate(Func<TagViewModel, Signalizations> filter = null)
        {
            Signalization = Signalizations.None;
            if (BlankType == BlankType.None && State != State.Init)
            {
                Signalization = Signalizations.Forbidden;
            }
            else if (BlankType == BlankType.Word)
            {
                var word = Blank as Word;
                if (word.IsTransient)
                    Signalization = Signalizations.NewWord;
            }

            if (filter != null)
            {
                Signalization |= filter(this);
            }
        }

        public static BlankType GetBlankType(object blank)
        {
            if (blank is Word)
                return BlankType.Word;
            if (blank is Comment)
                return BlankType.Comment;
            if (blank is string)
                return BlankType.Query;
            if (blank == null)
                return BlankType.None;
            if (blank is Measure)
                return BlankType.Measure;
            if (blank is IcdDisease)
                return BlankType.Icd;

            throw new ArgumentOutOfRangeException("blank");
        }

        public override string ToString()
        {
            return string.Format("tag q:{0}({3}) b:{1}({2})", Query, Blank, BlankType, State);
        }

        protected virtual void OnDeleted()
        {
            var h = Deleted;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        protected virtual void OnConverting(BlankType targetType)
        {
            var h = Converting;
            if (h != null)
            {
                h(this, new BlankTypeEventArgs(targetType));
            }
        }

        private void Reset()
        {
            Query = string.Empty;
            State = State.Init;

            // setting Blank sets State, so
            Signalization = null;
            _blank = null;
            OnPropertyChanged("Blank");
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(State != State.Completed || BlankType != BlankType.None
                || Signalization == null || Signalization == Signalizations.Forbidden); // завершенный тег → есть бланк (тег завершается после смены бланка) в поиске бланк мб пустой
            Contract.Invariant(State != State.Init || (BlankType == BlankType.None && Entity == null)); // в начальном состоянии → нет бланка и сущностей
            // при редактировании нет сущностей
        }

        public void OnDrop(DragEventArgs e)
        {
            autocomplete.OnDrop(e);
        }
    }

    [Serializable]
    public class TagEventArgs : EventArgs
    {
        public readonly TagViewModel tag;

        [DebuggerStepThrough]
        public TagEventArgs(TagViewModel tag)
        {
            this.tag = tag;
        }
    }


    [Serializable]
    public class BlankTypeEventArgs : EventArgs
    {
        public readonly BlankType type;

        [DebuggerStepThrough]
        public BlankTypeEventArgs(BlankType type)
        {
            this.type = type;
        }
    }


    [Serializable]
    public class TagData
    {
        public static readonly DataFormat DataFormat = DataFormats.GetDataFormat("tag");
        public List<IHrItemObject> ItemObjects { get; set; }
    }
}