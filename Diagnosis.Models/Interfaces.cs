using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    /// <summary>
    /// Сущность БД.
    /// </summary>
    public interface IEntity
    {
        bool IsDirty { get; set; }
        object Id { get; }
    }

    /// <summary>
    /// Доменный объект, не обязательно хранится в БД.
    /// </summary>
    public interface IDomainObject
    {
    }

    /// <summary>
    /// Сущность в элементе записи.
    /// </summary>
    public interface IHrItemObject : IDomainObject, IComparable<IHrItemObject> // Icd < Measure < Comment < Word
    {
    }

    /// <summary>
    /// Сущность МКБ
    /// </summary>
    public interface IIcdEntity
    {
        string Code { get; }
        string Title { get; }
        IIcdEntity Parent { get; }
    }

    /// <summary>
    /// Сущность, содержащая записи.
    /// </summary>
    public interface IHrsHolder : IDomainObject
    {
        event NotifyCollectionChangedEventHandler HealthRecordsChanged;
        IEnumerable<HealthRecord> HealthRecords { get; }
        object Actual { get; }
        HealthRecord AddHealthRecord();
        void RemoveHealthRecord(HealthRecord hr);
    }


    [Serializable]
    public class DomainEntityEventArgs : EventArgs
    {
        public readonly IDomainObject entity;

        [System.Diagnostics.DebuggerStepThrough]
        public DomainEntityEventArgs(IDomainObject entity)
        {
            this.entity = entity;
        }
    }

    [Serializable]
    public class HrsHolderEventArgs : EventArgs
    {
        public readonly IHrsHolder holder;

        [System.Diagnostics.DebuggerStepThrough]
        public HrsHolderEventArgs(IHrsHolder holder)
        {
            this.holder = holder;
        }
    }
}
