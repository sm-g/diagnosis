using System;
using System.Linq;

namespace Diagnosis.Models
{
    public class SpecialityVocabularies : EntityBase<Guid>
    {
        protected SpecialityVocabularies()
        {
        }

        public virtual Speciality Speciality { get; protected set; }

        public virtual Vocabulary Vocabulary { get; protected set; }
    }
}