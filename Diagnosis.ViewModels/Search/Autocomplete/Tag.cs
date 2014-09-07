using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Search.Autocomplete
{
    public enum TagStates
    {
        Init,
        Typing,
        Completed,
        // Leaved
    }
    public class Tag : ViewModelBase
    {
        private object _entity;
        private bool _focused;
        private string _query;
        private TagStates _state;
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
            State = TagStates.Init;
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
        /// То, что оказалось введенным после энтера - найденное слово, новый текст,
        /// число или ничего, если новые слова недопустимы
        /// Заготовка, из которой получаются сущности.
        /// </summary>
        public object EntityBlank
        {
            get
            {
                return _entity;
            }
            set
            {
                Contract.Requires(State != TagStates.Completed);
                if (_entity != value)
                {
                    _entity = value;
                    OnPropertyChanged(() => EntityBlank);

                    Console.WriteLine("{0} entity = {1}", this, value);
                }
                if (value != null)
                {
                    Query = value.ToString();
                }
                State = TagStates.Completed;
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
                    Console.WriteLine("{0} focused = {1}", this, value);
                    OnPropertyChanged(() => IsFocused);
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
                return State == TagStates.Completed &&
                       EntityBlank is string &&
                       !char.IsDigit((EntityBlank as string)[0]);
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
                    OnPropertyChanged(() => IsDeleteOnly);
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

                    State = TagStates.Typing;

                    _query = value;
                    OnPropertyChanged(() => Query);
                }
            }
        }

        public TagStates State
        {
            get { return _state; }
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    Console.WriteLine("{0} {1}", this, value);
                    if (value == TagStates.Completed && freezeOnComplete)
                    {
                        IsDeleteOnly = true;
                    }

                    OnPropertyChanged(() => IsNewWord);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Query, EntityBlank);
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
}
