using Diagnosis.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

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
        /// Некорректный тег (новое слово).
        /// </summary>
        Forbidden
    }

    public class Tag : ViewModelBase
    {
        public static readonly ILog logger = LogManager.GetLogger(typeof(Tag));
        private readonly bool freezeOnComplete;
        private object _blank;
        private bool _focused;
        private string _query;
        private States _state;
        private bool _isDeleteOnly;
        private Signalizations _signal;

        /// <summary>
        /// Создает тег с текстом в начальном состоянии.
        /// </summary>
        public Tag(string title, bool freezeOnComplete)
        {
            if (!string.IsNullOrEmpty(title))
                Query = title;

            this.freezeOnComplete = freezeOnComplete;
            State = States.Init;
        }

        /// <summary>
        /// Создает пустой тег.
        /// </summary>
        public Tag(bool freezeOnComplete)
            : this(null as string, freezeOnComplete)
        {
        }

        /// <summary>
        /// Создает тег с сущностью в завершенном состоянии.
        /// </summary>
        public Tag(IDomainEntity entity, bool deleteOnly)
        {
            Contract.Requires(entity != null);
            freezeOnComplete = deleteOnly;

            Blank = entity;
            Entities = new List<IDomainEntity>() { entity };
        }

        public event EventHandler Deleted;

        /// <summary>
        /// Типы сущностей в теге.
        /// </summary>
        public enum BlankTypes
        {
            None,
            /// <summary>
            /// Строка-запрос, может быть словом или измерением
            /// </summary>
            Query,
            Word,
            Measure,
        }

        /// <summary>
        /// Состояния тега.
        /// </summary>
        public enum States
        {
            Init,
            Typing,
            Completed
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(OnDeleted);
            }
        }

        /// <summary>
        /// Сущности, созданные из тега. При изменении запроса сбрасывается.
        /// </summary>
        public IEnumerable<IDomainEntity> Entities { get; internal set; }

        /// <summary>
        /// Заготовка, из которой получаются сущности.
        /// То, что оказалось введенным после энтера - найденное слово, текст запроса,
        /// число c единицей или ничего, если новые слова недопустимы.
        /// </summary>
        public object Blank
        {
            get
            {
                return _blank;
            }
            set
            {
                Contract.Requires(State != States.Completed);
                if (_blank != value)
                {
                    _blank = value;
                    OnPropertyChanged("Blank");
                    OnPropertyChanged("BlankType");

                    // logger.DebugFormat("{0} blank changed", this);
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
                    //  Debug.Print("{0} focused = {1}", this, value);
                    OnPropertyChanged("IsFocused");
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
                    if (value == States.Completed && freezeOnComplete)
                    {
                        IsDeleteOnly = true;
                    }

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

        public void Validate()
        {
            Signalization = Signalizations.None;
            if (BlankType == Tag.BlankTypes.None && State != Tag.States.Init)
            {
                Signalization = Signalizations.Forbidden;
            }
            else if (BlankType == Tag.BlankTypes.Query)
            {
                var str = Blank as string;
                if (!Recognizer.IsMeasure(str))
                {
                    Signalization = Signalizations.NewWord;
                }
                else if (Recognizer.SplitMeasureQuery(str).Item2 != "")
                {
                    Signalization = Signalizations.PartialMeasure;
                }
            }
        }

        public static BlankTypes GetBlankType(object blank)
        {
            if (blank is Word)
                return BlankTypes.Word;
            if (blank is string)
                return BlankTypes.Query;
            if (blank == null)
                return BlankTypes.None;
            if (blank is Recognizer.NumbersWithUom || blank is Measure)
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