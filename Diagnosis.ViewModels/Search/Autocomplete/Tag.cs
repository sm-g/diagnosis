using Diagnosis.Models;
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
        PartialMeasure,
        /// <summary>
        /// Некорректный тег (новый без заготовки).
        /// </summary>
        Forbidden
    }

    public class Tag : ViewModelBase, IDropTarget
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Tag));
        readonly Autocomplete autocomplete;
        private readonly bool canConvert;
        private object _blank;
        private bool _focused;
        private string _query;
        private States _state;
        private bool _isDeleteOnly;
        private bool _isLast;
        private bool _listItemFocused;
        private bool _selected;
        private bool _draggable;
        private Signalizations _signal;

        /// <summary>
        /// Создает пустой тег.
        /// </summary>
        public Tag(Autocomplete parent, bool canConvert)
        {
            Contract.Ensures(State == States.Init);

            this.canConvert = canConvert;
            this.autocomplete = parent;
        }

        /// <summary>
        /// Создает тег с запросом.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="query"></param>
        /// <param name="canConvert"></param>
        public Tag(Autocomplete parent, string query, bool canConvert)
        {
            Contract.Ensures(State == States.Typing);

            this.canConvert = canConvert;
            this.autocomplete = parent;
            Query = query;
        }


        /// <summary>
        /// Создает тег с сущностями.
        /// </summary>
        public Tag(Autocomplete parent, IHrItemObject item, bool canConvert)
        {
            Contract.Requires(item != null);
            Contract.Ensures(State == States.Completed);

            this.canConvert = canConvert;
            this.autocomplete = parent;

            Blank = item;
            Entities = new List<IHrItemObject>() { item };
        }

        public event EventHandler Deleted;
        public event EventHandler Converting;
        /// <summary>
        /// Типы заготовок в теге.
        /// </summary>
        public enum BlankTypes
        {
            None,
            /// <summary>
            /// Строка-запрос
            /// </summary>
            Query,
            Comment,
            Word,
            Measure
        }

        /// <summary>
        /// Состояния тега.
        /// </summary>
        public enum States
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

        public string Query
        {
            get
            {
                return _query;
            }
            set
            {
                if (Query != value)
                {
                    logger.DebugFormat("query ={0}", value);
                    Contract.Assume(!IsDeleteOnly);

                    State = States.Typing;

                    // show drag when type in last
                    if (IsLast && !value.IsNullOrEmpty())
                        IsDraggable = true;

                    _query = value;
                    Entities = null;
                    OnPropertyChanged("Query");
                }
            }
        }

        public States State
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
        /// Сущности, созданные из тега. При изменении запроса или бланка сбрасывается.
        /// </summary>
        public IEnumerable<IHrItemObject> Entities { get; internal set; }

        /// <summary>
        /// Заготовка, из которой получаются сущности.
        /// То, что оказалось введенным - найденное слово, текст запроса или ничего.
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
                    logger.DebugFormat("blank ={0}", value);

                    _blank = value;
                    OnPropertyChanged("Blank");
                    OnPropertyChanged("BlankType");
                }
                if (value != null)
                {
                    Query = value.ToString();
                }
                State = States.Completed;
            }
        }

        /// <summary>
        /// Тип заготовки.
        /// </summary>
        public BlankTypes BlankType
        {
            get
            {
                return GetBlankType(Blank);
            }
        }
        public RelayCommand ConvertCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    OnConverting(EventArgs.Empty);
                },
                () => canConvert && State != States.Init && !Query.IsNullOrEmpty() && !IsDeleteOnly);
            }
        }
        #region AutocompleteRelated

        public RelayCommand AddLeftCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    autocomplete.Add(this, true);
                });
            }
        }
        public RelayCommand AddRightCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    autocomplete.Add(this, false);
                }, () => !IsLast);
            }
        }
        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!IsLast)
                    {
                        OnDeleted();
                    }
                    else
                    {
                        Query = null;
                        State = States.Init;
                    }
                });
            }
        }

        public Signalizations Signalization
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
        /// Единственный последний тег для набора текста по умолчанию. 
        /// Не удаляется.
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
                    logger.DebugFormat("{0} focused = {1}, focused2 = {2}", this, value, _listItemFocused);
                    OnPropertyChanged("IsTextBoxFocused");
                }
            }
        }
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
                    logger.DebugFormat("{0} focused2 = {1}, focused = {2}", this, value, _focused);

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
                    logger.DebugFormat("{0} selected {1}", this, value);
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
        #endregion

        public void Validate()
        {
            Signalization = Signalizations.None;
            if (BlankType == Tag.BlankTypes.None && State != Tag.States.Init)
            {
                Signalization = Signalizations.Forbidden;
            }
            else if (BlankType == Tag.BlankTypes.Word)
            {
                var word = Blank as Word;
                if (word.IsTransient)
                    Signalization = Signalizations.NewWord;
            }
        }

        public static BlankTypes GetBlankType(object blank)
        {
            if (blank is Word)
                return BlankTypes.Word;
            if (blank is Comment)
                return BlankTypes.Comment;
            if (blank is string)
                return BlankTypes.Query;
            if (blank == null)
                return BlankTypes.None;
            if (blank is Measure)
                return BlankTypes.Measure;

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

        protected virtual void OnConverting(EventArgs e)
        {
            var h = Converting;
            if (h != null)
            {
                h(this, e);
            }
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            // Contract.Invariant(State != States.Completed || BlankType != BlankTypes.None); // завершенный тег → есть бланк (тег завершается после смены бланка) в поиске бланк мб пустой
            Contract.Invariant(State != States.Init || (BlankType == BlankTypes.None && Entities == null)); // в начальном состоянии → нет бланка и сущностей
            // при редактирвоаии нет сущностей
        }

        public void OnDrop(DragEventArgs e)
        {
            autocomplete.OnDrop(e);
        }
    }

    [Serializable]
    public class TagEventArgs : EventArgs
    {
        public readonly Tag tag;

        [DebuggerStepThrough]
        public TagEventArgs(Tag tag)
        {
            this.tag = tag;
        }
    }


    [Serializable]
    public class TagData
    {
        public static readonly DataFormat DataFormat = DataFormats.GetDataFormat("tag");
        public string Query { get; set; }
        public object Blank { get; set; }
        public List<IHrItemObject> ItemObjects { get; set; }
    }
}