using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class Symptom : EntityBase
    {
        ISet<Word> words = new HashSet<Word>();
        ISet<SymptomWords> symptomWords = new HashSet<SymptomWords>();

        public virtual IcdDisease Disease { get; set; }
        public virtual Category DefaultCategory { get; set; }
        public virtual bool IsDiagnosis { get; set; }
        public virtual IEnumerable<Word> Words
        {
            get
            {
                return words;
            }
        }

        public virtual IEnumerable<SymptomWords> SymptomWords
        {
            get
            {
                return symptomWords;
            }
        }

        public Symptom(IEnumerable<Word> words)
        {
            Contract.Requires(words != null);
            Contract.Requires(words.Count() > 0);

            this.words = new HashSet<Word>(words);
        }

        protected Symptom() { }

        public override string ToString()
        {
            return string.Join(". ", Words.OrderBy(w => w.Priority).Select(w => w.Title));

        }
    }
}
