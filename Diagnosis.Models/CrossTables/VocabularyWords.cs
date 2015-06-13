using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class VocabularyWords : EntityBase<Guid>
    {
        public VocabularyWords(Vocabulary v, Word w)
        {
            Contract.Requires(v != null);
            Contract.Requires(w != null);

            Vocabulary = v;
            Word = w;
        }
        protected VocabularyWords()
        {
        }

        public virtual Word Word { get; set; }

        public virtual Vocabulary Vocabulary { get; set; } // public for replace fk after sync
    }
}