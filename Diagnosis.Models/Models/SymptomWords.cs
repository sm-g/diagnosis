using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class SymptomWords
    {
        public virtual int Id { get; protected set; }
        public virtual Symptom Symptom { get; protected set; }
        public virtual Word Word { get; protected set; }

        protected SymptomWords() { }
    }
}
