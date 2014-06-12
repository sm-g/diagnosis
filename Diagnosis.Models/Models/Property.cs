using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Models
{
    public class Property : IEntity
    {
        ISet<PropertyValue> values = new HashSet<PropertyValue>();
        ISet<PatientRecordProperty> patientProperties = new HashSet<PatientRecordProperty>();

        public virtual int Id { get; protected set; }
        public virtual string Title { get; set; }
        public virtual ReadOnlyCollection<PropertyValue> Values
        {
            get
            {
                return new ReadOnlyCollection<PropertyValue>(
                    new List<PropertyValue>(values));
            }
        }
        public virtual ReadOnlyCollection<PatientRecordProperty> PatientProperties
        {
            get
            {
                return new ReadOnlyCollection<PatientRecordProperty>(
                    new List<PatientRecordProperty>(patientProperties));
            }
        }

        public virtual void AddValue(string value)
        {
            Contract.Requires(!String.IsNullOrEmpty(value));

            var pv = new PropertyValue(this, value);

            if (!values.Contains(pv, PropertyValue.equalityComparer))
            {
                values.Add(pv);
            }
        }

        public virtual void RemoveValue(PropertyValue value)
        {
            values.Remove(value);
        }

        public Property(string title)
        {
            Contract.Requires(!String.IsNullOrEmpty(title));
            Title = title;
        }
        protected Property()
        {
        }

        public override string ToString()
        {
            return Title;
        }
    }
}