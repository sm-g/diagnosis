using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class PropertyValue
    {
        Iesi.Collections.Generic.ISet<PatientProperty> patientProperties;
        internal static IEqualityComparer<PropertyValue> equalityComparer = new PropertyValueEqualityComparer();

        public virtual int Id { get; protected set; }
        public virtual string Title { get; set; }
        public virtual Property Property { get; set; }
        public virtual ReadOnlyCollection<PatientProperty> PatientProperties
        {
            get
            {
                return new ReadOnlyCollection<PatientProperty>(
                    new List<PatientProperty>(patientProperties));
            }
        }

        public PropertyValue(String value, Property property)
            : base()
        {
            Title = value;
            Property = property;
        }

        protected PropertyValue()
        {
            patientProperties = new Iesi.Collections.Generic.HashedSet<PatientProperty>();
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
