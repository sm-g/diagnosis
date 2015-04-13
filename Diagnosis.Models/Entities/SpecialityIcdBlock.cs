using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class SpecialityIcdBlocks : EntityBase<Guid>
    {
        public SpecialityIcdBlocks(Speciality s, IcdBlock b)
        {
            Contract.Requires(s != null);
            Contract.Requires(b != null);

            Speciality = s;
            IcdBlock = b;
        }
        protected SpecialityIcdBlocks()
        {
        }

        public virtual Speciality Speciality { get; set; } // public for replace fk after sync

        public virtual IcdBlock IcdBlock { get; set; }
    }
}