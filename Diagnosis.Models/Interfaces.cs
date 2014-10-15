using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    /// <summary>
    /// Доменный объект, не обязательно хранится в БД.
    /// </summary>
    public interface IDomainEntity
    {
    }

    /// <summary>
    /// Сущность в элементе записи.
    /// </summary>
    public interface IHrItemObject
    {
    }

    /// <summary>
    /// Сущность, содержащая записи.
    /// </summary>
    public interface IHrsHolder : IDomainEntity
    {
        event NotifyCollectionChangedEventHandler HealthRecordsChanged;
        IEnumerable<HealthRecord> HealthRecords { get; }
        HealthRecord AddHealthRecord();
        void RemoveHealthRecord(HealthRecord hr);
    }


    [Serializable]
    public class DomainEntityEventArgs : EventArgs
    {
        public readonly IDomainEntity entity;

        [System.Diagnostics.DebuggerStepThrough]
        public DomainEntityEventArgs(IDomainEntity entity)
        {
            this.entity = entity;
        }
    }
}
