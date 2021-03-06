﻿using Diagnosis.Common;
using Diagnosis.Models.Validators;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;

namespace Diagnosis.Models
{
    [Serializable]
    public class Word : ValidatableEntity<Guid>, IDomainObject, IHrItemObject, IComparable<Word>, IDeletable
    {
        [NonSerialized]
        private ISet<Word> children = new HashSet<Word>();

        [NonSerialized]
        private ISet<VocabularyWords> vocabularyWords = new HashSet<VocabularyWords>();

        [NonSerialized]
        private ISet<CritWords> critWords = new HashSet<CritWords>();

        [NonSerialized]
        private IList<HealthRecord> healthRecords = new List<HealthRecord>(); // many-2-many bag

        [NonSerialized]
        private Word _parent;

        private string _title;
        private Uom _uom;

        [NonSerialized]
        private Many2ManyHelper<VocabularyWords, Vocabulary> vwHelper;

        [NonSerialized]
        private Many2ManyHelper<CritWords, CritBase> crwHelper;

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
                Contract.Requires(value != null);
                var filtered = value.Prettify().Truncate(Length.WordTitle);
                SetProperty(ref _title, filtered, () => Title);
            }
        }

        public virtual Word Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value, () => Parent); }
        }

        /// <summary>
        /// Cамая частая единица на клиенте с этим словом.
        /// </summary>
        public virtual Uom Uom
        {
            get { return _uom; }
            set
            {
                if (value == _uom) return;
                if (value == Uom.Null) value = null;
                _uom = value;
            }
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

        public virtual IEnumerable<CritBase> Crits
        {
            get
            {
                return CrwHelper.Values;
            }
        }

        public virtual IEnumerable<CritWords> CritWords
        {
            get { return critWords; }
        }

        public virtual IEnumerable<Word> Children
        {
            get { return children; }
        }

        public virtual IEnumerable<HealthRecord> HealthRecords
        {
            get { return healthRecords; }
        }

        private Many2ManyHelper<VocabularyWords, Vocabulary> VwHelper
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

        private Many2ManyHelper<CritWords, CritBase> CrwHelper
        {
            get
            {
                if (crwHelper == null)
                {
                    crwHelper = new Many2ManyHelper<CritWords, CritBase>(
                        critWords,
                        x => x.Word == this,
                        x => x.Crit);
                }
                return crwHelper;
            }
        }

        /// <summary>
        /// Вызвать перед удалением слова.
        /// </summary>
        public virtual void OnDelete()
        {
            Vocabularies.ForEach(x => x.RemoveWord(this));
            HealthRecords
                .Select(x => x.Doctor)
                .Distinct()
                .ForEach(x => x.RemoveWordFromCache(this));
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
            else
                return new HrItemObjectComparer().Compare(this, hio);
        }

        public virtual int CompareTo(Word other)
        {
            if (other == null) return 1;

            // несохраненные могут быть с одним заголовком, !equals, but 0 == CompareTo()
            return StringComparer.OrdinalIgnoreCase.Compare(this.Title, other.Title);
        }

        public override ValidationResult SelfValidate()
        {
            return new WordValidator().Validate(this);
        }

        // for refresh state of many-2-many relations

        public virtual bool IsEmpty()
        {
            return !HealthRecords.Any() &&
                !Crits.Any();
        }

        protected internal virtual void RemoveVoc(Vocabulary voc)
        {
            Contract.Requires(voc != null);
            Contract.Requires(!voc.Words.Contains(this));
            Contract.Ensures(!VwHelper.Values.Contains(voc));
            VwHelper.Remove(voc);
        }

        protected internal virtual void AddVocWords(VocabularyWords vw)
        {
            Contract.Requires(vw != null);
            Contract.Requires(vw.Vocabulary.Words.Contains(this));
            VwHelper.Add(vw);
        }

        protected internal virtual void RemoveCrit(CritBase crit)
        {
            Contract.Requires(crit != null);
            Contract.Requires(!crit.Words.Contains(this));
            Contract.Ensures(!CrwHelper.Values.Contains(crit));
            CrwHelper.Remove(crit);
        }

        protected internal virtual void AddCritWords(CritWords crw)
        {
            Contract.Requires(crw != null);
            Contract.Requires(crw.Crit.Words.Contains(this));
            CrwHelper.Add(crw);
        }

        protected internal virtual void AddHr(HealthRecord hr)
        {
            Contract.Requires(hr != null);
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
            children = new HashSet<Word>();
            vocabularyWords = new HashSet<VocabularyWords>();
        }
    }
}