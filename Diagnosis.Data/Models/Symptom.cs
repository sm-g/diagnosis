using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class Symptom
    {
        ISet<Word> words = new HashSet<Word>();

        public virtual int Id { get; protected set; }
        public virtual IcdDisease Disease { get; set; }
        public virtual Category DefaultCategory { get; set; }
        public virtual ReadOnlyCollection<Word> Words
        {
            get
            {
                return new ReadOnlyCollection<Word>(
                    new List<Word>(words));
            }
        }

        //public virtual void AddWord(Word word)
        //{
        //    Contract.Requires(word != null);
        //    words.Add(word);
        //}

        //public virtual void RemoveWord(Word word)
        //{
        //    Contract.Requires(word != null);
        //    words.Remove(word);
        //}

        public Symptom(IEnumerable<Word> words)
        {
            Contract.Requires(words != null);
            Contract.Requires(words.Count() > 0);

            this.words = new HashSet<Word>(words);
        }

        protected Symptom() { }
    }
}
