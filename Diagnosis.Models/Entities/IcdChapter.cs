using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class IcdChapter : EntityBase, IDomainEntity
    {
        Iesi.Collections.Generic.ISet<IcdBlock> iclBlocks;

        public virtual string Title { get; protected set; }
        public virtual string Code { get; protected set; }

        public virtual IEnumerable<IcdBlock> IclBlocks
        {
            get
            {
                return iclBlocks;
            }
        }

        protected IcdChapter() { }
    }
}
