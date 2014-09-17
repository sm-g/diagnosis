using Diagnosis.Core;
using PixelMEDIA.PixelCore.Helpers;
using System;
using System.Collections;
using System.ComponentModel;

namespace Diagnosis.Models
{
    /// <summary>
    /// Доменный объект, не обязательно хранится в БД.
    /// </summary>
    public interface IDomainEntity
    {
    }

    /// <summary>
    /// Сущность БД.
    /// </summary>
    public class EntityBase : NotifyPropertyChangedBase, IEditableObject
    {
        private int? cachedHashCode;
        private bool _isDeleted;
        private EditableObjectHelper _editHelper;
        private object _syncRoot = new object();
        private bool? wasChangedBeforeEdit;

        public EntityBase()
        {
            PropertyChanged += (s, e) =>
            {
                if (InEdit)
                {
                    IsDirty = true;
                }
            };
        }

        public virtual int Id { get; protected set; }

        /// <summary>
        /// Указывает, что сущность помечена на удаление.
        /// </summary>
        public virtual bool IsDeleted
        {
            get
            {
                return _isDeleted;
            }
            set
            {
                if (_isDeleted != value)
                {
                    _isDeleted = value;
                    OnPropertyChanged(() => IsDeleted);
                }
            }
        }

        /// <summary>
        /// Указывает, что есть несохраненные изменения.
        /// </summary>
        public virtual bool IsDirty
        {
            get;
            set;
        }

        /// <summary>
        /// Указывает, что сущность новая, не сохранена в БД.
        /// </summary>
        public virtual bool IsTransient
        {
            get { return Id == 0; }
        }

        public virtual bool InEdit
        {
            get { return (_editHelper != null ? _editHelper.InEdit : false); }
        }

        protected EditableObjectHelper EditHelper
        {
            get
            {
                if (_editHelper == null)
                {
                    lock (_syncRoot)
                    {
                        if (_editHelper == null)
                        {
                            _editHelper = new EditableObjectHelper(this);
                        }
                    }
                }
                return _editHelper;
            }
        }

        void IEditableObject.BeginEdit()
        {
            lock (_syncRoot)
            {
                if (!wasChangedBeforeEdit.HasValue)
                    wasChangedBeforeEdit = IsDirty;
                EditHelper.BeginEdit();
            }
        }

        void IEditableObject.CancelEdit()
        {
            lock (_syncRoot)
            {
                EditHelper.CancelEdit();

                IsDirty = wasChangedBeforeEdit.Value;
                wasChangedBeforeEdit = null;
            }
        }

        void IEditableObject.EndEdit()
        {
            lock (_syncRoot)
            {
                if (!EditHelper.InEdit)
                    return;

                EditHelper.EndEdit();

                wasChangedBeforeEdit = null;
            }
        }

        public override bool Equals(object obj)
        {
            return EntityEquals(obj as EntityBase);
        }

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            cachedHashCode = IsTransient ? base.GetHashCode() : Id.GetHashCode();
            return cachedHashCode.Value;
        }

        protected bool EntityEquals(EntityBase other)
        {
            if (other == null)
            {
                return false;
            }
            else if (IsTransient && other.IsTransient)
            {
                // both entities are not saved
                return ReferenceEquals(this, other);
            }
            else
            {
                // one of entities saved.
                return Id == other.Id;
            }
        }

        // Maintain equality operator semantics for entities.
        public static bool operator ==(EntityBase x, EntityBase y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Object.Equals(x, y);
        }

        // Maintain inequality operator semantics for entities.
        public static bool operator !=(EntityBase x, EntityBase y)
        {
            return !(x == y);
        }
    }

    public class EditableObjectHelper
    {
        private object _syncRoot = new object();
        private IEditableObject _master;
        private IDictionary _originalValues;
        private bool _inEdit;
        private bool _inOriginalValuesReset;

        public bool InEdit
        {
            get { return _inEdit; }
        }

        public bool InOriginalValuesReset
        {
            get { return _inOriginalValuesReset; }
        }

        [Browsable(false)]
        protected IDictionary OriginalValues
        {
            get
            {
                if (_originalValues == null)
                {
                    _originalValues = new Hashtable();
                }
                return _originalValues;
            }
        }

        public EditableObjectHelper(IEditableObject master)
        {
            if (master == null)
                throw new ArgumentNullException("master");

            _master = master;
        }

        public void BeginEdit()
        {
            lock (_syncRoot)
            {
                if (_inEdit)
                    return;

                _inEdit = true;
            }
        }

        public void Edit(string propertyName, object value)
        {
            lock (_syncRoot)
            {
                if (_inOriginalValuesReset)
                    return;

                if (_inEdit)
                {
                    // сохраняем значение свойства до вызова BeginEdit()
                    IDictionary originalValues = OriginalValues;
                    if (!originalValues.Contains(propertyName))
                    {
                        originalValues.Add(
                            propertyName,
                            value);
                    }
                }
            }
        }

        public void CancelEdit()
        {
            lock (_syncRoot)
            {
                if (!_inEdit)
                    return;

                try
                {
                    _inOriginalValuesReset = true;

                    foreach (DictionaryEntry entry in OriginalValues)
                    {
                        ReflectionHelper.SetPropertyValue(
                            _master, (string)entry.Key, entry.Value);
                    }
                }
                finally
                {
                    _inOriginalValuesReset = false;
                }

                _inEdit = false;

                OriginalValues.Clear();
            }
        }

        public void EndEdit()
        {
            lock (_syncRoot)
            {
                if (!_inEdit)
                    return;

                _inEdit = false;

                OriginalValues.Clear();
            }
        }
    }
}