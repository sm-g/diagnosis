using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class PropertyValue
    {
        public string Value { get; set; }
        public Property Property { get; set; }

        public PropertyValue(String value, Property property)
        {
            Value = value;
            Property = property;
        }
    }
}
