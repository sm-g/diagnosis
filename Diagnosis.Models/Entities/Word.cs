using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Word : EntityBase, IDomainObject, IHrItemObject, IComparable<Word>
    {
        private Iesi.Collections.Generic.ISet<Word> children;
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

        public virtual HrCategory DefaultCategory { get; set; }

        public virtual Word Parent { get; set; }

        public virtual ObservableCollection<Word> Children
        {
            get
            {
                return new ObservableCollection<Word>(children);
            }
        }

        public Word(string title)
        {
            Contract.Requires(!string.IsNullOrEmpty(title));
            Title = title;
        }

        protected Word()
        {
        }

        public override string ToString()
        {
            return Title;
        }

        public virtual int CompareTo(IHrItemObject hio)
        {
            var word = hio as Word;
            if (word != null)
                return this.CompareTo(word);

            return 1; // 'biggest'
        }

        public virtual int CompareTo(Word other)
        {
            return this.Title.CompareTo(other.Title);
        }
    }
}