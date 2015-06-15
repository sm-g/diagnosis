using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    /// <summary>
    /// Сущность БД.
    /// </summary>
    public interface IEntity : INotifyPropertyChanged
    {
        bool IsDirty { get; set; }
        bool IsTransient { get; }
        object Id { get; }
        object Actual { get; }
    }

    /// <summary>
    /// Доменный объект, не обязательно хранится в БД.
    /// </summary>
    public interface IDomainObject
    {
    }

    public interface IDeletable
    {
        bool IsEmpty();
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
    [ContractClass(typeof(ContractForIIcdEntity))]
    public interface IIcdEntity : IComparable<IIcdEntity> // disease < block < chapter
    {
        string Code { get; }
        string Title { get; }
        IIcdEntity Parent { get; }
    }

    /// <summary>
    /// Пользователь системы
    /// </summary>
    public interface IUser : IEntity
    {
        Passport Passport { get; }
    }

    /// <summary>
    /// Сущность, содержащая записи.
    /// </summary>
    [ContractClass(typeof(ContractForIHrsHolder))]
    public interface IHrsHolder : IEntity, IDomainObject, IComparable<IHrsHolder>, IDeletable // App < Course < Patient
    {
        event NotifyCollectionChangedEventHandler HealthRecordsChanged;
        IEnumerable<HealthRecord> HealthRecords { get; }
        HealthRecord AddHealthRecord(Doctor author);
        void RemoveHealthRecord(HealthRecord hr);
    }

    /// <summary>
    /// Сущность, содержащая критерии.
    /// </summary>
    public interface ICrit : IEntity, IDomainObject, IDeletable
    {
        string Description { get; }
    }

    internal interface IHaveAuditInformation : IEntity
    {
        DateTime UpdatedAt { get; set; }
        DateTime CreatedAt { get; set; }
    }
}
