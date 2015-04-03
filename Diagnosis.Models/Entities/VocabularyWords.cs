using System;
using System.Linq;

namespace Diagnosis.Models
{
    public class VocabularyWords : EntityBase<Guid>
    {
        protected VocabularyWords()
        {
        }

        public virtual Word Word { get; protected set; }

        public virtual Vocabulary Vocabulary { get; set; } // public for replace fk after sync
    }
}