using Diagnosis.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Diagnosis.Common;

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

    public class Tag : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Tag));
        private readonly bool canConvert;
        private object _blank;
        private bool _focused;
        private string _query;
        private States _state;
        private bool _isDeleteOnly;
        private bool _isLast;
        private Signalizations _signal;

        /// <summary>
        /// Создает пустой тег.
        /// </summary>
        public Tag(bool canConvert)
        {
            this.canConvert = canConvert;
            State = States.Init;
        }

        /// <summary>
        /// Создает тег с сущностями в завершенном состоянии.
        /// </summary>
        public Tag(IHrItemObject item, bool canConvert)
        {
            Contract.Requires(item != null);
            this.canConvert = canConvert;

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

        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(OnDeleted,
                    () => !IsLast);
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

        public bool IsFocused
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
                    logger.DebugFormat("{0} focused = {1}", this, value);
                    OnPropertyChanged("IsFocused");
                }
            }
        }

        private bool _selected;
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
                    OnPropertyChanged(() => IsSelected);
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
                    OnPropertyChanged(() => IsLast);
                }
            }
        }

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
            Contract.Invariant(State != States.Completed || BlankType != BlankTypes.None); // завершенный тег → есть бланк (тег завершается после смены бланка)
            Contract.Invariant(State != States.Init || (BlankType == BlankTypes.None && Entities == null)); // в начальном состоянии → нет бланка и сущностей
            // при редактирвоаии нет сущностей
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
}