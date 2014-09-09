using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Diagnosis.Models
{
    public class IcdBlock : EntityBase
    {
        ISet<IcdDisease> icdDiseases;
        ISet<SpecialityIcdBlock> specialityIcdBlocks;

        public virtual IcdChapter IcdChapter { get; protected set; }
        public virtual string Title { get; protected set; }
        public virtual string Code { get; protected set; }
        public virtual ReadOnlyCollection<IcdDisease> IcdDiseases
        {
            get
            {
                return new ReadOnlyCollection<IcdDisease>(
                    new List<IcdDisease>(icdDiseases));
            }
        }
        public virtual IEnumerable<SpecialityIcdBlock> SpecialityIcdBlocks
        {
            get { return new List<SpecialityIcdBlock>(specialityIcdBlocks); }
        }

        protected IcdBlock() { }
    }
}
