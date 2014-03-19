using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class PropertyValue
    {
        ISet<PatientProperty> patientProperties = new HashSet<PatientProperty>();
        internal static IEqualityComparer<PropertyValue> equalityComparer = new PropertyValueEqualityComparer();

        public virtual int Id { get; protected set; }
        public virtual string Title { get; set; }
        public virtual Property Property { get; protected set; }
        public virtual ReadOnlyCollection<PatientProperty> PatientProperties
        {
            get
            {
                return new ReadOnlyCollection<PatientProperty>(
                    new List<PatientProperty>(patientProperties));
            }
        }

        public virtual void Delete()
        {
            Property.RemoveValue(this);
        }

        public PropertyValue(Property property, string value)
        {
            Contract.Requires(property != null);
            Contract.Requires(!String.IsNullOrEmpty(value));
            Title = value;
            Property = property;
        }

        protected PropertyValue()
        {
        }
    }

    class PropertyValueEqualityComparer : EqualityComparer<PropertyValue>
    {
        public override bool Equals(PropertyValue x, PropertyValue y)
        {
            return x.Title == y.Title && x.Property.Title == y.Property.Title;
        }
        public override int GetHashCode(PropertyValue obj)
        {
            return base.GetHashCode();
        }
    }

    public class EmptyPropertyValue : PropertyValue
    {
        public EmptyPropertyValue(Property property)
            : base()
        {
            Title = "123";
            Property = property;
        }
    }
}
