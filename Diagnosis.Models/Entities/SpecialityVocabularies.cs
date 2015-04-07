using System;
using System.Linq;

namespace Diagnosis.Models
{
    public class SpecialityVocabularies : EntityBase<Guid>
    {
        protected SpecialityVocabularies()
        {
        }

        public virtual Speciality Speciality { get; set; } // public for replace fk after sync

        public virtual Vocabulary Vocabulary { get; set; }
    }
}