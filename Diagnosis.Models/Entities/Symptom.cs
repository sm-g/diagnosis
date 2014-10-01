using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class Symptom : EntityBase, IDomainEntity
    {
        IList<Word> words;
        Iesi.Collections.Generic.ISet<SymptomWords> symptomWords;

        public virtual IcdDisease Disease { get; set; }
        public virtual HrCategory DefaultCategory { get; set; }
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

        public Symptom(ICollection<Word> words)
        {
            Contract.Requires(words != null);
            Contract.Requires(words.Count() > 0);

            this.words = new List<Word>(words);
        }

        protected Symptom() { }

        public override string ToString()
        {
            return string.Join(". ", Words.OrderBy(w => w.Priority).Select(w => w.Title));

        }
    }
}
