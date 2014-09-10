using Diagnosis.Core;
using System;

namespace Diagnosis.Models
{
    public interface IEntity
    {
    }

    public class EntityBase : NotifyPropertyChangedBase, IEntity
    {
        private int? cachedHashCode;

        public virtual int Id { get; protected set; }

        /// <summary>
        /// Указывает, что сущность помечена на удаление.
        /// </summary>
        public virtual bool IsDeleted
        {
            get;
            set;
        }

        /// <summary>
        /// Указывает, что есть несохраненные изменения.
        /// </summary>
        public virtual bool IsDirty
        {
            get;
            protected set;
        }

        public virtual bool IsTransient
        {
            get { return Id == 0; }
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
}