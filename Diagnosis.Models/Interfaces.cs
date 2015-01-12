using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
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
        object Actual { get; }
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
    public interface IHrsHolder : IEntity, IDomainObject, IComparable<IHrsHolder> // App < Course < Patient
    {
        event NotifyCollectionChangedEventHandler HealthRecordsChanged;
        IEnumerable<HealthRecord> HealthRecords { get; }
        HealthRecord AddHealthRecord(Doctor author);
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


    [Serializable]
    public class UserEventArgs : EventArgs
    {
        public readonly IUser user;

        [System.Diagnostics.DebuggerStepThrough]
        public UserEventArgs(IUser user)
        {
            this.user = user;
        }
    }

    [ContractClassFor(typeof(IHrsHolder))]
    abstract class ContractForIHrsHolder : IHrsHolder
    {

        event NotifyCollectionChangedEventHandler IHrsHolder.HealthRecordsChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        IEnumerable<HealthRecord> IHrsHolder.HealthRecords { get { throw new NotImplementedException(); } }

        bool IEntity.IsDirty { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        object IEntity.Id { get { throw new NotImplementedException(); } }

        object IEntity.Actual { get { throw new NotImplementedException(); } }

        HealthRecord IHrsHolder.AddHealthRecord(Doctor author)
        {
            IHrsHolder test = this;
            Contract.Requires(author != null);
            Contract.Ensures(test.HealthRecords.Count() == Contract.OldValue(test.HealthRecords.Count()) + 1);

            return null;
        }

        void IHrsHolder.RemoveHealthRecord(HealthRecord hr)
        {
            IHrsHolder test = this;
            Contract.Ensures(test.HealthRecords.Count() <= Contract.OldValue(test.HealthRecords.Count()));
        }
        int IComparable<IHrsHolder>.CompareTo(IHrsHolder other) { throw new NotImplementedException(); }
    }
}
