using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Diagnosis.Models
{
    public class IcdBlock
    {
        ISet<IcdDisease> icdDiseases = new HashSet<IcdDisease>();

        public virtual int Id { get; protected set; }
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

        protected IcdBlock() { }
    }
}
