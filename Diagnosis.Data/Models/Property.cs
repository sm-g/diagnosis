using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Models
{
    public class Property
    {
        Iesi.Collections.Generic.ISet<PropertyValue> values;
        Iesi.Collections.Generic.ISet<PatientProperty> patientProperties;

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
        public virtual ReadOnlyCollection<PatientProperty> PatientProperties
        {
            get
            {
                return new ReadOnlyCollection<PatientProperty>(
                    new List<PatientProperty>(patientProperties));
            }
        }

        public virtual void AddValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("value");

            var pv = new PropertyValue(value, this);

            if (!values.Contains(pv, PropertyValue.equalityComparer))
            {
                values.Add(pv);
            }
        }

        public Property(string title)
            : base()
        {
            Title = title;
        }
        protected Property()
        {
            values = new Iesi.Collections.Generic.HashedSet<PropertyValue>();
            patientProperties = new Iesi.Collections.Generic.HashedSet<PatientProperty>();
        }
    }
}