using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    [ContractClassFor(typeof(IHrsHolder))]
    internal abstract class ContractForIHrsHolder : IHrsHolder
    {
        public event NotifyCollectionChangedEventHandler HealthRecordsChanged = delegate { };

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

        int IComparable<IHrsHolder>.CompareTo(IHrsHolder other)
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public bool IsTransient
        {
            get { throw new NotImplementedException(); }
        }
    }

    [ContractClassFor(typeof(IIcdEntity))]
    internal abstract class ContractForIIcdEntity : IIcdEntity
    {
        string IIcdEntity.Code
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                Contract.Ensures(Contract.Result<string>().Length > 0);
                Contract.Ensures(Contract.Result<string>().Length <= Length.IcdCode); // block (A00-A05)
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