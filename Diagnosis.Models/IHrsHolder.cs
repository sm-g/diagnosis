using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Diagnosis.Models
{
    public interface IHrsHolder
    {
        event NotifyCollectionChangedEventHandler HealthRecordsChanged;
        IEnumerable<HealthRecord> HealthRecords { get; }
        HealthRecord AddHealthRecord();
        void RemoveHealthRecord(HealthRecord hr);
    }
}
