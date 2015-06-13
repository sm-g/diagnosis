using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class SpecialityVocabularies : EntityBase<Guid>
    {
        public SpecialityVocabularies(Speciality s, Vocabulary v)
        {
            Contract.Requires(s != null);
            Contract.Requires(v != null);

            Speciality = s;
            Vocabulary = v;
        }
        protected SpecialityVocabularies()
        {
        }

        public virtual Speciality Speciality { get; set; } // public for replace fk after sync

        public virtual Vocabulary Vocabulary { get; set; }
    }
}