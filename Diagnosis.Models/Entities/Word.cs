using Diagnosis.Models.Validators;
using FluentValidation.Results;
using Iesi.Collections.Generic;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    [Serializable]
    public class Word : ValidatableEntity<Guid>, IDomainObject, IHrItemObject, IComparable<Word>
    {
        [NonSerialized]
        private Iesi.Collections.Generic.ISet<Word> children = new HashedSet<Word>();
        [NonSerialized]
        private Iesi.Collections.Generic.ISet<Vocabulary> vocabularies = new HashedSet<Vocabulary>();

        [NonSerialized]
        private IList<HealthRecord> healthRecords = new List<HealthRecord>(); // many-2-many bag

        [NonSerialized]
        private Word _parent;

        private string _title;

        public Word(string title)
        {
            Contract.Requires(title != null); // empty when adding new

            Title = title;
        }

        protected Word()
        {
        }

        public virtual string Title
        {
            get { return _title; }
            set
            {
                var filtered = value.Replace(Environment.NewLine, " ").Replace('\t', ' ').Trim();
                SetProperty(ref _title, filtered, () => Title);
            }
        }
        public virtual IEnumerable<Vocabulary> Vocabularies
        {
            get { return vocabularies; }
        }
        public virtual Word Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value, () => Parent); }
        }

        public virtual IEnumerable<Word> Children
        {
            get { return children; }
        }

        public virtual IEnumerable<HealthRecord> HealthRecords
        {
            get { return healthRecords; }
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
            // несохраненные могут быть с одним заголовком, !equals, but 0 == CompareTo()
            return this.Title.CompareTo(other.Title);
        }

        public override ValidationResult SelfValidate()
        {
            return new WordValidator().Validate(this);
        }

        // for refresh state of many-2-many relations

        internal protected virtual void RemoveVoc(Vocabulary voc)
        {
            Contract.Requires(!voc.Words.Contains(this));
            vocabularies.Remove(voc);
        }

        internal protected virtual void AddVoc(Vocabulary voc)
        {
            Contract.Requires(voc.Words.Contains(this));
            vocabularies.Add(voc);
        }

        internal protected virtual void AddHr(HealthRecord hr)
        {
            Contract.Requires(hr.Words.Contains(this));
            healthRecords.Add(hr);
        }

        internal protected virtual void RemoveHr(HealthRecord hr)
        {
            // при удалении записи слова не удаляются отдельно
            // при удалении элемента слово уже удалено
            healthRecords.Remove(hr);
        }
    }
}