using Diagnosis.Models.Validators;
using FluentValidation.Results;
using Iesi.Collections.Generic;
using System;
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
        private IList<HealthRecord> healthRecords = new List<HealthRecord>(); // many-2-many bag

        [NonSerialized]
        private Word _parent;

        [NonSerialized]
        private HrCategory _defCat;

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

        public virtual HrCategory DefaultCategory
        {
            get { return _defCat; }
            set { SetProperty(ref _defCat, value, () => DefaultCategory); }
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
    }
}