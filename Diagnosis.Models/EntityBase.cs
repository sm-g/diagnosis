using Diagnosis.Common;
using PixelMEDIA.PixelCore.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("Diagnosis.Data")]

namespace Diagnosis.Models
{
    [Serializable]
    /// <summary>
    /// Сущность БД c Id типа int или Giud.
    /// </summary>
    public abstract class EntityBase<TId> : NotifyPropertyChangedBase, IEditableObject, IEntity
    {
        [NonSerialized]
        private int? cachedHashCode;
        [NonSerialized]
        private EditableObjectHelper _editHelper;
        [NonSerialized]
        private object _syncRoot = new object();
        [NonSerialized]
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

        public virtual TId Id { get; protected set; }

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
            get { return EqualityComparer<TId>.Default.Equals(Id, default(TId)); }
        }

        public virtual bool InEdit
        {
            get { return (_editHelper != null ? _editHelper.InEdit : false); }
        }

        /// <summary>
        /// Returns entity itself (not its proxy).
        /// </summary>
        public virtual object Actual { get { return this; } }

        bool IEntity.IsDirty
        {
            get { return IsDirty; }
            set { IsDirty = value; }
        }

        object IEntity.Id
        {
            get { return Id; }
        }

        protected internal virtual EditableObjectHelper EditHelper
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

        // Maintain equality operator semantics for entities.
        public static bool operator ==(EntityBase<TId> x, EntityBase<TId> y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Object.Equals(x, y);
        }

        // Maintain inequality operator semantics for entities.
        public static bool operator !=(EntityBase<TId> x, EntityBase<TId> y)
        {
            return !(x == y);
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
            return EntityEquals(obj as EntityBase<TId>);
        }

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            cachedHashCode = IsTransient ? base.GetHashCode() : Id.GetHashCode();
            return cachedHashCode.Value;
        }

        protected bool EntityEquals(EntityBase<TId> other)
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
                return EqualityComparer<TId>.Default.Equals(Id, other.Id);
            }
        }

        /// <summary>
        /// Edits, sets the property and notifies listeners only when necessary.
        /// </summary>
        protected bool SetProperty<T>(ref T storage, T value, string propertyName)
        {
            if (object.Equals(storage, value)) return false;

            EditHelper.Edit(propertyName, storage);
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        /// <summary>
        /// Edits, sets the property and notifies listeners only when necessary.
        /// </summary>
        protected bool SetProperty<T>(ref T storage, T value, Expression<Func<T>> propertyExpression)
        {
            if (object.Equals(storage, value)) return false;

            EditHelper.Edit<T, TId>(propertyExpression);
            storage = value;
            OnPropertyChanged(propertyExpression);
            return true;
        }
    }
}