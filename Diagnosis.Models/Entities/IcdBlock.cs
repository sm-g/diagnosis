using Iesi.Collections.Generic;
using System.Collections.Generic;

namespace Diagnosis.Models
{
    public class IcdBlock : EntityBase<int>, IDomainObject, IIcdEntity
    {
        private Iesi.Collections.Generic.ISet<IcdDisease> icdDiseases = new HashedSet<IcdDisease>();
        private Iesi.Collections.Generic.ISet<SpecialityIcdBlocks> specialityIcdBlocks = new HashedSet<SpecialityIcdBlocks>();

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
            if (other == null)
                return -1;
            return this.Code.CompareTo(other.Code);
        }
    }
}