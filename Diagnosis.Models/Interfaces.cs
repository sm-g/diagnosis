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
    [ContractClass(typeof(ContractForIIcdEntity))]
    public interface IIcdEntity : IComparable<IIcdEntity>
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

    public interface ICrit
    {

    }

    internal interface IHaveAuditInformation : IEntity
    {
        DateTime UpdatedAt { get; set; }
        DateTime CreatedAt { get; set; }
    }

    #region EventArgs

    [Serializable]
    public class HealthRecordEventArgs : EventArgs
    {
        public readonly HealthRecord hr;

        [System.Diagnostics.DebuggerStepThrough]
        public HealthRecordEventArgs(HealthRecord hr)
        {
            this.hr = hr;
        }
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
    #endregion

    [ContractClassFor(typeof(IHrsHolder))]
    abstract class ContractForIHrsHolder : IHrsHolder
    {

        event NotifyCollectionChangedEventHandler IHrsHolder.HealthRecordsChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        IEnumerable<HealthRecord> IHrsHolder.HealthRecords
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<HealthRecord>>().IsOrdered(x => x.Ord));
                return null;
            }
        }

        bool IEntity.IsDirty { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        object IEntity.Id { get { throw new NotImplementedException(); } }

        object IEntity.Actual { get { throw new NotImplementedException(); } }

        HealthRecord IHrsHolder.AddHealthRecord(Doctor author)
        {
            IHrsHolder test = this;
            Contract.Requires(author != null);
            Contract.Ensures(test.HealthRecords.Count() == Contract.OldValue(test.HealthRecords.Count()) + 1);
            Contract.Ensures(test.HealthRecords.Contains(Contract.Result<HealthRecord>()));
            Contract.Ensures(Contract.Result<HealthRecord>().Words.All(x => x.HealthRecords.Contains(Contract.Result<HealthRecord>())));
            Contract.Ensures(author.HealthRecords.Contains(Contract.Result<HealthRecord>()));

            return null;
        }

        void IHrsHolder.RemoveHealthRecord(HealthRecord hr)
        {
            IHrsHolder test = this;
            Contract.Ensures(test.HealthRecords.Count() <= Contract.OldValue(test.HealthRecords.Count()));
            Contract.Ensures(!test.HealthRecords.Contains(hr));
            Contract.Ensures(hr.Words.All(x => !x.HealthRecords.Contains(hr)));
        }
        int IComparable<IHrsHolder>.CompareTo(IHrsHolder other) { throw new NotImplementedException(); }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }

    [ContractClassFor(typeof(IIcdEntity))]
    abstract class ContractForIIcdEntity : IIcdEntity
    {
        string IIcdEntity.Code
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                Contract.Ensures(Contract.Result<string>().Length > 0);
                Contract.Ensures(Contract.Result<string>().Length <= 9); // block (A00-A05)
                return "A00.1";
            }
        }

        string IIcdEntity.Title
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                Contract.Ensures(Contract.Result<string>().Length > 0);
                return "0";
            }
        }

        IIcdEntity IIcdEntity.Parent
        {
            get
            {
                IIcdEntity test = this;
                Contract.Ensures(Contract.Result<IIcdEntity>() != null || (test as IcdChapter) != null);
                return null;
            }
        }

        int IComparable<IIcdEntity>.CompareTo(IIcdEntity other)
        {
            throw new NotImplementedException();
        }
    }
}
