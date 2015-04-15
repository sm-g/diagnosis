using Diagnosis.Common;
using Diagnosis.Models;
using System;

namespace Diagnosis.Data
{
    [Serializable]
    internal class EntityChecker : IEntityCrudTracker
    {
        public void Delete(IEntity entity)
        {
        }

        public void Insert(IEntity entity)
        {
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