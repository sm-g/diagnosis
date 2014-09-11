﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class PropertyValue : EntityBase, IDomainEntity
    {
        Iesi.Collections.Generic.ISet<PatientRecordProperty> patientProperties;
        internal static IEqualityComparer<PropertyValue> equalityComparer = new PropertyValueEqualityComparer();

        public virtual string Title { get; set; }
        public virtual Property Property { get; protected set; }
        public virtual IEnumerable<PatientRecordProperty> PatientProperties
        {
            get
            {
                return patientProperties;
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

        public override string ToString()
        {
            return Title;
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
