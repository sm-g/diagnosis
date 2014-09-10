using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Diagnosis.Models
{
    public class IcdBlock : EntityBase, IDomainEntity
    {
        ISet<IcdDisease> icdDiseases;
        ISet<SpecialityIcdBlock> specialityIcdBlocks;

        public virtual IcdChapter IcdChapter { get; protected set; }
        public virtual string Title { get; protected set; }
        public virtual string Code { get; protected set; }
        public virtual IEnumerable<IcdDisease> IcdDiseases
        {
            get
            {
                return icdDiseases;
            }
        }
        public virtual IEnumerable<SpecialityIcdBlock> SpecialityIcdBlocks
        {
            get { return specialityIcdBlocks; }
        }

        protected IcdBlock() { }
    }
}
