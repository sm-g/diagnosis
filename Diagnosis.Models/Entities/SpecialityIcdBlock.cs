using System;
using System.Linq;

namespace Diagnosis.Models
{
    public class SpecialityIcdBlocks : EntityBase<Guid>
    {
        protected SpecialityIcdBlocks()
        {
        }

        public virtual Speciality Speciality { get; protected set; }

        public virtual IcdBlock IcdBlock { get; protected set; }
    }
}