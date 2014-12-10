using Diagnosis.Models.Validators;
using FluentValidation.Results;
using Iesi.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    [Serializable]
    public class Word : ValidatableEntity<Guid>, IDomainObject, IHrItemObject, IComparable<Word>
    {
        [NonSerialized]
        private Iesi.Collections.Generic.ISet<Word> children = new HashedSet<Word>();
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
                var filtered = value.Replace(Environment.NewLine, " ").Replace('\t', ' ');
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

        public virtual ObservableCollection<Word> Children
        {
            get
            {
                return new ObservableCollection<Word>(children);
            }
        }

        public Word(string title)
        {
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
            return this.Title.CompareTo(other.Title); // несохраненные могут быть с одним заголовком
        }

        public override ValidationResult SelfValidate()
        {
            return new WordValidator().Validate(this);
        }
    }
}