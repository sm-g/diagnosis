using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class PatientProperty
    {
        public virtual int Id { get; protected set; }
        public virtual Patient Patient { get; set; }
        public virtual PropertyValue Value { get; set; }
        public virtual Property Property { get; set; }
    }
}
