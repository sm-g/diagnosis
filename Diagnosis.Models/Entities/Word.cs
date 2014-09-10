using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Word : EntityBase
    {
        private ISet<Symptom> symptoms;
        private ISet<SymptomWords> symptomWords;
        private ISet<Word> children;
        private string _title;

        public virtual string Title
        {
            get { return _title; }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                _title = value;
            }
        }

        public virtual byte Priority { get; set; }

        public virtual Category DefaultCategory { get; set; }

        public virtual Word Parent { get; set; }
        public virtual ObservableCollection<Word> Children
        {
            get
            {
                return new ObservableCollection<Word>(children);
            }
        }

        public virtual IEnumerable<Symptom> Symptoms
        {
            get
            {
                return symptoms;
            }
        }

        public virtual IEnumerable<SymptomWords> SymptomWords
        {
            get
            {
                return symptomWords;
            }
        }

        public Word(string title, byte priority = 0)
        {
            Contract.Requires(!string.IsNullOrEmpty(title));
            Title = title;
            Priority = priority;
        }

        protected Word()
        {
        }

        public override string ToString()
        {
            return Title;
        }
    }

    public class CompareWord : IComparer<Word>
    {
        public int Compare(Word x, Word y)
        {
            if (x.Priority != y.Priority)
            {
                return x.Priority.CompareTo(y.Priority);
            }
            else
            {
                return x.Title.CompareTo(y.Title);
            }
        }
    }
}