using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class SpecialityIcdBlock
    {
        public virtual int Id { get; protected set; }

        public virtual Speciality Speciality { get; protected set; }
        public virtual IcdBlock IcdBlock { get; protected set; }

        protected SpecialityIcdBlock() { }
    }
}
