using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Core;

namespace Diagnosis.Models
{
    public interface IEntity
    {
    }

    public class EntityBase : NotifyPropertyChangedBase, IEntity
    {
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
    }
}
