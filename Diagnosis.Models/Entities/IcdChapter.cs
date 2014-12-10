using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class IcdChapter : EntityBase<int>, IDomainObject, IIcdEntity
    {
        Iesi.Collections.Generic.ISet<IcdBlock> iclBlocks = new HashedSet<IcdBlock>();

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
        protected IcdChapter() { }
    }
}
