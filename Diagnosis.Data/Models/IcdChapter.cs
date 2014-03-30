using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class IcdChapter
    {
        ISet<IcdBlock> iclBlocks = new HashSet<IcdBlock>();

        public virtual int Id { get; protected set; }
        public virtual string Title { get; protected set; }
        public virtual string Code { get; protected set; }

        public virtual ReadOnlyCollection<IcdBlock> IclBlocks
        {
            get
            {
                return new ReadOnlyCollection<IcdBlock>(
                    new List<IcdBlock>(iclBlocks));
            }
        }

        protected IcdChapter() { }
    }
}
