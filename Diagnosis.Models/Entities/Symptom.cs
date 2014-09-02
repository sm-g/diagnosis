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
        public virtual ReadOnlyCollection<Word> Words
        {
            get
            {
                return new ReadOnlyCollection<Word>(
                    new List<Word>(words));
            }
        }

        public virtual ReadOnlyCollection<SymptomWords> SymptomWords
        {
            get
            {
                return new ReadOnlyCollection<SymptomWords>(
                    new List<SymptomWords>(symptomWords));
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
