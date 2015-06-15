using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Diagnostics;

namespace Diagnosis.Data
{
    [Serializable]
    internal class EntityChecker : IEntityCrudTracker
    {
        public void Delete(IEntity entity)
        {
            this.Send(Event.EntityDeleted, entity.AsParams(MessageKeys.Entity));
        }

        public void Insert(IEntity entity)
        {
            if (entity is IValidatable)
            {
                Debug.Assert((entity as IValidatable).IsValid());
            }

            entity.IsDirty = false;
            if (entity is Word)
            {
                this.Send(Event.WordPersisted, entity.AsParams(MessageKeys.Word));
            }
        }

        public void Update(IEntity entity)
        {
            entity.IsDirty = false;
        }

        public void Load(IEntity entity)
        {
            if (entity is HealthRecord)
            {
                var hr = entity as HealthRecord;
                hr.FixDescribedAtAfterLoad();
            }
        }
    }
}