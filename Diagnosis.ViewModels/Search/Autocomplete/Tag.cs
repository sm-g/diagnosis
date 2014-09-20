using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Diagnosis.ViewModels.Search.Autocomplete
{
    public class Tag : ViewModelBase
    {
        private readonly bool freezeOnComplete;
        private object _blank;
        private bool _focused;
        private string _query;
        private States _state;
        private bool _isDeleteOnly;
        private bool _partialMeasure;
        private bool _isNewWord;
        private bool _isValid;
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
            Completed,
            // Leaved
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

                    Debug.Print("{0} blank = {1}", this, value);
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
                    Debug.Print("{0} focused = {1}", this, value);
                    OnPropertyChanged("IsFocused");
                }
            }
        }

        /// <summary>
        /// Тег содержит новое слово.
        /// </summary>
        public bool IsNewWord
        {
            get
            {
                return _isNewWord;
            }
            set
            {
                if (_isNewWord != value)
                {
                    _isNewWord = value;
                    OnPropertyChanged(() => IsNewWord);
                }
            }
        }

        /// <summary>
        /// Частичное измерение с неправильной единицей.
        /// </summary>
        public bool IsPartialMeasure
        {
            get
            {
                return _partialMeasure;
            }
            set
            {
                if (_partialMeasure != value)
                {
                    _partialMeasure = value;
                    OnPropertyChanged(() => IsPartialMeasure);
                }
            }
        }

        /// <summary>
        /// Некорректный тег (новое слово).
        /// </summary>
        public bool IsInvalid
        {
            get
            {
                return _isValid;
            }
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged(() => IsInvalid);
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
                    Debug.Print("{0} {1}", this, value);
                    if (value == States.Completed && freezeOnComplete)
                    {
                        IsDeleteOnly = true;
                    }

                    OnPropertyChanged("State");
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