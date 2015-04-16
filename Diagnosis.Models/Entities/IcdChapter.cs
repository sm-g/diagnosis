using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Models
{
    public class IcdChapter : EntityBase<int>, IDomainObject, IIcdEntity
    {
        private ISet<IcdBlock> iclBlocks = new HashSet<IcdBlock>();

        protected IcdChapter()
        {
        }

        public virtual string Title { get; protected set; }

        public virtual string Code { get; protected set; }

        public virtual IEnumerable<IcdBlock> IclBlocks
        {
            get
            {
                return iclBlocks;
            }
        }

        IIcdEntity IIcdEntity.Parent
        {
            get { return null; }
        }

        public virtual int CompareTo(IIcdEntity other)
        {
            if (other == null)
                return -1;
            return this.Code.CompareTo(other.Code);
        }
    }
}