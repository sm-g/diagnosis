using System.Collections.Generic;

namespace Diagnosis.Models
{
    public class IcdBlock : EntityBase<int>, IDomainObject, IIcdEntity
    {
        private ISet<IcdDisease> icdDiseases = new HashSet<IcdDisease>();
        private ISet<SpecialityIcdBlocks> specialityIcdBlocks = new HashSet<SpecialityIcdBlocks>();

        protected IcdBlock()
        {
        }

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

        public virtual IEnumerable<SpecialityIcdBlocks> SpecialityIcdBlocks
        {
            get { return specialityIcdBlocks; }
        }

        IIcdEntity IIcdEntity.Parent
        {
            get { return IcdChapter; }
        }

        public virtual int CompareTo(IIcdEntity other)
        {
            return new IcdEntityComparer().Compare(this, other);
        }
    }
}