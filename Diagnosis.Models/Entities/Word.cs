using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    [Serializable]
    public class Word : EntityBase, IDomainObject, IHrItemObject, IComparable<Word>
    {
        [NonSerialized]
        private Iesi.Collections.Generic.ISet<Word> children;
        [NonSerialized]
        private Word _parent;
        [NonSerialized]
        private HrCategory _defCat;

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

        public virtual HrCategory DefaultCategory
        {
            get { return _defCat; }
            set { _defCat = value; }
        }

        public virtual Word Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

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