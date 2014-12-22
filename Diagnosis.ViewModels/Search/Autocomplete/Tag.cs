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

    public class Tag : ViewModelBase, IDropTarget
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Tag));
        readonly Autocomplete autocomplete;
        private object _blank;
        private bool _focused;
        private string _query;
        private States _state;
        private bool _isDeleteOnly;
        private bool _isLast;
        private bool _listItemFocused;
        private bool _selected;
        private bool _draggable;
        private Signalizations? _signal;

        /// <summary>
        /// Создает пустой тег.
        /// </summary>
        public Tag(Autocomplete parent)
        {
            Contract.Ensures(State == States.Init);

            this.autocomplete = parent;
        }

        /// <summary>
        /// Создает тег с запросом.
        /// </summary>
        public Tag(Autocomplete parent, string query)
        {
            Contract.Ensures(State == States.Typing);

            this.autocomplete = parent;
            Query = query;
        }


        /// <summary>
        /// Создает тег с сущностями.
        /// </summary>
        public Tag(Autocomplete parent, IHrItemObject item)
        {
            Contract.Requires(item != null);
            Contract.Ensures(State == States.Completed);

            this.autocomplete = parent;

            Blank = item;
            Entities = new List<IHrItemObject>() { item };
        }

        public event EventHandler Deleted;
        public event EventHandler<BlankTypeEventArgs> Converting;
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
            Measure,
            Icd
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

        /// <summary>
        /// Текстовое представление
        /// </summary>
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
        /// Копируются.
        /// </summary>
        public IEnumerable<IHrItemObject> Entities { get; internal set; }

        /// <summary>
        /// Заготовка, из которой получаются сущности.
        /// То, что оказалось введенным - найденное слово, текст запроса, МКБ или ничего.
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
                    logger.DebugFormat("blank = {0} ({1})", value, GetBlankType(value));

                    _blank = value;
                    OnPropertyChanged("Blank");
                    OnPropertyChanged("BlankType");
                    OnPropertyChanged(() => Focusable);
                }
                if (value != null)
                {
                    Query = value.ToString();
                }
                State = States.Completed;
                Signalization = null;
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
                        Query = null;
                        State = States.Init;
                        _blank = null;
                        OnPropertyChanged("Blank");
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
                    this.Send(Events.SendToSearch, Blank.ToEnumerable().AsParams(MessageKeys.HrItemObjects));
                },
                () => autocomplete.WithSendToSearch);
            }
        }
        public RelayCommand<BlankTypes> ConvertToCommand
        {
            get
            {
                return new RelayCommand<BlankTypes>((t) =>
                {
                    OnConverting(t);
                },
                (t) => autocomplete.WithConvert && t != BlankType && State != States.Init && !Query.IsNullOrEmpty() && !IsDeleteOnly);
            }
        }

        public RelayCommand AddLeftCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    autocomplete.Add(this, true);
                }, () => !autocomplete.SingleTag);
            }
        }
        public RelayCommand AddRightCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    autocomplete.Add(this, false);
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
                    logger.DebugFormat("{0} focused = {1}, focused2 = {2}", this, value, _listItemFocused);
                    OnPropertyChanged("IsTextBoxFocused");
                }
            }
        }

        public bool Focusable
        {
            get
            {
                return BlankType != BlankTypes.Icd   // редактируются через отдельный редактор
                    && BlankType != BlankTypes.Measure;
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

        public void Validate(Func<Tag, Signalizations> filter = null)
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

            if (filter != null)
            {
                Signalization |= filter(this);
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
            if (blank is IcdDisease)
                return BlankTypes.Icd;

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

        protected virtual void OnConverting(BlankTypes targetType)
        {
            var h = Converting;
            if (h != null)
            {
                h(this, new BlankTypeEventArgs(targetType));
            }
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(State != States.Completed || BlankType != BlankTypes.None
                || Signalization == null || Signalization == Signalizations.Forbidden); // завершенный тег → есть бланк (тег завершается после смены бланка) в поиске бланк мб пустой
            Contract.Invariant(State != States.Init || (BlankType == BlankTypes.None && Entities == null)); // в начальном состоянии → нет бланка и сущностей
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
        public readonly Tag tag;

        [DebuggerStepThrough]
        public TagEventArgs(Tag tag)
        {
            this.tag = tag;
        }
    }


    [Serializable]
    public class BlankTypeEventArgs : EventArgs
    {
        public readonly Tag.BlankTypes type;

        [DebuggerStepThrough]
        public BlankTypeEventArgs(Tag.BlankTypes type)
        {
            this.type = type;
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