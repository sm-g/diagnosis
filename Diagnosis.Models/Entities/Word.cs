using Diagnosis.Common;
using Diagnosis.Models.Validators;
using FluentValidation.Results;
using Iesi.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;

namespace Diagnosis.Models
{
    [Serializable]
    public class Word : ValidatableEntity<Guid>, IDomainObject, IHrItemObject, IComparable<Word>
    {
        [NonSerialized]
        private Iesi.Collections.Generic.ISet<Word> children = new HashedSet<Word>();

        [NonSerialized]
        private Iesi.Collections.Generic.ISet<VocabularyWords> vocabularyWords = new HashedSet<VocabularyWords>();

        [NonSerialized]
        private IList<HealthRecord> healthRecords = new List<HealthRecord>(); // many-2-many bag

        [NonSerialized]
        private Word _parent;

        private string _title;

        [NonSerialized]
        private Many2ManyHelper<Models.VocabularyWords, Vocabulary> vwHelper;

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

        public virtual Word Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value, () => Parent); }
        }

        public virtual IEnumerable<Vocabulary> Vocabularies
        {
            get
            {
                return VwHelper.Values;
            }
        }

        public virtual IEnumerable<VocabularyWords> VocabularyWords
        {
            get { return vocabularyWords; }
        }

        public virtual IEnumerable<Word> Children
        {
            get { return children; }
        }

        public virtual IEnumerable<HealthRecord> HealthRecords
        {
            get { return healthRecords; }
        }

        private Many2ManyHelper<Models.VocabularyWords, Vocabulary> VwHelper
        {
            get
            {
                if (vwHelper == null)
                {
                    vwHelper = new Many2ManyHelper<VocabularyWords, Vocabulary>(
                        vocabularyWords,
                        x => x.Word == this,
                        x => x.Vocabulary);
                }
                return vwHelper;
            }
        }

        /// <summary>
        /// Вызвать перед удалением слова.
        /// </summary>
        public virtual void OnDelete()
        {
            Vocabularies.ForEach(x => x.RemoveWord(this));
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

        protected internal virtual void RemoveVoc(Vocabulary voc)
        {
            Contract.Requires(!voc.Words.Contains(this));
            Contract.Ensures(!VwHelper.Values.Contains(voc));
            VwHelper.Remove(voc);
        }
        protected internal virtual void AddVocWords(VocabularyWords vw)
        {
            Contract.Requires(vw.Vocabulary.Words.Contains(this));
            VwHelper.Add(vw);
        }

        protected internal virtual void AddHr(HealthRecord hr)
        {
            Contract.Requires(hr.Words.Contains(this));
            healthRecords.Add(hr);
        }

        protected internal virtual void RemoveHr(HealthRecord hr)
        {
            // при удалении записи слова не удаляются отдельно
            // при удалении элемента слово уже удалено
            healthRecords.Remove(hr);
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            healthRecords = new List<HealthRecord>(); // many-2-many bag
            children = new HashedSet<Word>();
            vocabularyWords = new HashedSet<VocabularyWords>();
        }
    }
}