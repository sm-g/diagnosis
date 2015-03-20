using Diagnosis.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Diagnosis.Common;
using System.Windows;
using GongSolutions.Wpf.DragDrop;

namespace Diagnosis.ViewModels.Autocomplete
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

    public class TagViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TagViewModel));
        readonly IAutocompleteViewModel autocomplete;
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
        private Confidence _confidence;
        private VisibleRelayCommand sendToSearch;
        private VisibleRelayCommand<BlankType> convertTo;
        private VisibleRelayCommand tooggleConfid;
        private VisibleRelayCommand addLeft;
        private VisibleRelayCommand addRight;

        /// <summary>
        /// Создает пустой тег.
        /// </summary>
        public TagViewModel(IAutocompleteViewModel parent)
        {
            Contract.Requires(parent != null);
            Contract.Ensures(State == State.Init);

            this.autocomplete = parent;
            Reset();
            IsDraggable = !autocomplete.SingleTag;
        }

        /// <summary>
        /// Создает тег с запросом.
        /// </summary>
        public TagViewModel(IAutocompleteViewModel parent, string query)
        {
            Contract.Requires(parent != null);
            Contract.Ensures(State == State.Typing);

            this.autocomplete = parent;
            Query = query;
            IsDraggable = !autocomplete.SingleTag;
        }


        /// <summary>
        /// Создает тег с сущностями.
        /// </summary>
        public TagViewModel(IAutocompleteViewModel parent, IHrItemObject item)
        {
            Contract.Requires(parent != null);
            Contract.Requires(item != null);
            Contract.Ensures(State == State.Completed);

            this.autocomplete = parent;

            Blank = item;
            Entity = item;
            IsDraggable = !autocomplete.SingleTag;
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


                    _query = value ?? string.Empty;
                    Entity = null;

                    // show drag when type in last
                    if (IsLast)
                        IsDraggable = !value.IsNullOrEmpty();

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

        public Confidence Confidence
        {
            get
            {
                return _confidence;
            }
            set
            {
                if (_confidence != value)
                {
                    _confidence = value;
                    OnPropertyChanged(() => Confidence);
                }
            }
        }

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public BlankType BlankType
        {
            get
            {
                if (Blank is Word)
                    return BlankType.Word;
                if (Blank is Comment)
                    return BlankType.Comment;
                if (Blank is string)
                    return BlankType.Query;
                if (Blank is Measure)
                    return BlankType.Measure;
                if (Blank is IcdDisease)
                    return BlankType.Icd;

                return BlankType.None;
            }
            set
            {
                // to correct IsChecked binding must be TwoWay
                // replace ConvertTo with 4 bools?
            }
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

        public VisibleRelayCommand SendToSearchCommand
        {
            get
            {
                return sendToSearch ?? (sendToSearch = new VisibleRelayCommand(() =>
                {
                    autocomplete.SendToSearchCommand.Execute(this);
                },
                () => autocomplete.WithSendToSearch)
                {
                    IsVisible = autocomplete.WithSendToSearch
                });
            }
        }
        public VisibleRelayCommand<BlankType> ConvertToCommand
        {
            get
            {
                return convertTo ?? (convertTo = new VisibleRelayCommand<BlankType>((t) =>
                {
                    OnConverting(t);
                    OnPropertyChanged(() => BlankType);
                },
                (t) => autocomplete.WithConvert && t != BlankType && !IsDeleteOnly)
                {
                    IsVisible = autocomplete.WithConvert
                });
            }
        }
        public VisibleRelayCommand ToggleConfidenceCommand
        {
            get
            {
                return tooggleConfid ?? (tooggleConfid = new VisibleRelayCommand(() =>
                {
                    autocomplete.ToggleConfidenceCommand.Execute(null);
                },
                () => autocomplete.WithConfidence)
                {
                    IsVisible = autocomplete.WithConfidence
                });
            }
        }

        public VisibleRelayCommand AddLeftCommand
        {
            get
            {
                return addLeft ?? (addLeft = new VisibleRelayCommand(() =>
                {
                    autocomplete.AddAndEditTag(this, true);
                }, () => !autocomplete.SingleTag)
                {
                    IsVisible = !autocomplete.SingleTag
                });
            }
        }
        public VisibleRelayCommand AddRightCommand
        {
            get
            {
                return addRight ?? (addRight = new VisibleRelayCommand(() =>
                {
                    autocomplete.AddAndEditTag(this, false);
                }, () => !IsLast && !autocomplete.SingleTag)
                {
                    IsVisible = !autocomplete.SingleTag // все равно показываем пункт меню, если иногда можно
                });
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
                    IsDraggable = !value || !Query.IsNullOrEmpty() || autocomplete.SingleTag;

                    OnPropertyChanged(() => IsLast);
                }
            }
        }

        public void OnDrop(DragEventArgs e)
        {
            autocomplete.OnDrop(e);
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
                    logger.DebugFormat("{0} focusedTxt = {1}, focusedItem = {2}", this, value, _listItemFocused);
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
        public void ToggleEditState()
        {
            Contract.Ensures(IsTextBoxFocused != Contract.OldValue(IsTextBoxFocused));
            Contract.Ensures(IsTextBoxFocused != IsListItemFocused); // или выделен или редактируется

            if (IsTextBoxFocused)
            {
                IsListItemFocused = true;
                IsTextBoxFocused = false;
            }
            else
            {
                IsTextBoxFocused = true;
                IsListItemFocused = false;
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
            // если можно только слова
            if (filter != null)
            {
                Signalization |= filter(this);
            }
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
            OnPropertyChanged("BlankType");
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(State != State.Completed || BlankType != BlankType.None
                || Signalization == null || Signalization == Signalizations.Forbidden); // завершенный тег → есть бланк (тег завершается после смены бланка) в поиске бланк мб пустой
            Contract.Invariant(State != State.Init || (BlankType == BlankType.None && Entity == null)); // в начальном состоянии → нет бланка и сущностей
            // при редактировании нет сущностей

            Contract.Invariant(BlankType != BlankType.Query); // заготовка всегда сущность, если есть
            Contract.Invariant((IsLast && Query.IsNullOrEmpty()) != IsDraggable || autocomplete.SingleTag); // последний пустой без маркера переноса
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

        IList<ConfindenceHrItemObject> itemobjects;
        public IList<ConfindenceHrItemObject> ItemObjects { get { return itemobjects; } }

        public TagData(IList<ConfindenceHrItemObject> itemobjects)
        {
            this.itemobjects = itemobjects;
        }
    }
}