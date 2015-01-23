using Diagnosis.Models;
using System;
using System.Linq;

namespace Diagnosis.Data
{
    internal interface IEntityCrudTracker
    {
        void Load(IEntity entity);

        void Insert(IEntity entity);

        void Update(IEntity entity);

        void Delete(IEntity entity);
    }
}