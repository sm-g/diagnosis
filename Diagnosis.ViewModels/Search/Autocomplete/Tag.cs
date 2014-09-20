using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Search.Autocomplete
{

    public class Tag : ViewModelBase
    {
        private object _entity;
        private bool _focused;
        private string _query;
        private States _state;
        private bool _isDeleteOnly;
        readonly bool freezeOnComplete;
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
            : this(null, freezeOnComplete)
        {
        }

        /// <summary>
        /// Создает тег с сущностью в завершенном состоянии.
        /// </summary>
        public Tag(object entity, bool deleteOnly)
        {
            Contract.Requires(entity != null);
            freezeOnComplete = deleteOnly;

            EntityBlank = entity;
        }

        public event EventHandler Deleted;

        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(OnDeleted);
            }
        }

        /// <summary>
        /// Заготовка, из которой получаются сущности.
        /// То, что оказалось введенным после энтера - найденное слово, текст запроса,
        /// число c единицей или ничего, если новые слова недопустимы.
        /// </summary>
        public object EntityBlank
        {
            get
            {
                return _entity;
            }
            set
            {
                Contract.Requires(State != States.Completed);
                if (_entity != value)
                {
                    _entity = value;
                    OnPropertyChanged("EntityBlank");
                    OnPropertyChanged("EntityBlankType");

                    Debug.Print("{0} entity = {1}", this, value);
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
        public BlankTypes EntityBlankType
        {
            get
            {
                return GetBlankType(EntityBlank);
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
                return EntityBlankType == BlankTypes.Query &&
                       !Recognizer.IsMeasure(Query);
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

        public override string ToString()
        {
            return string.Format("{0} {1}", Query, EntityBlank);
        }

        public static BlankTypes GetBlankType(object blank)
        {
            if (blank is Word)
                return BlankTypes.Word;
            if (blank is string)
                return BlankTypes.Query;
            if (blank == null)
                return BlankTypes.None;
            if (blank is Recognizer.NumbersWithUom)
                return BlankTypes.Measure;

            throw new ArgumentOutOfRangeException("blank");
        }

        protected virtual void OnDeleted()
        {
            var h = Deleted;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }
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
            /// <summary>
            /// 
            /// </summary>
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
