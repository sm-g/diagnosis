using Diagnosis.Models.Validators;
using FluentValidation.Results;
using Iesi.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Vocabulary : ValidatableEntity<Guid>, IDomainObject
    {
        public static string CustomTitle = "Пользовательский";

        private Iesi.Collections.Generic.ISet<WordTemplate> wordTemplates = new HashedSet<WordTemplate>();
        private Iesi.Collections.Generic.ISet<Speciality> specialities = new HashedSet<Speciality>();
        private IList<Word> words = new List<Word>(); // many-2-many
        private string _title;

        public Vocabulary(string title, Doctor d = null)
        {
            Contract.Requires(title != null);

            Title = title;
            if (d != null)
            {
                Doctor = d;
                IsCustom = true;
            }
        }

        protected Vocabulary()
        {
        }


        public virtual event NotifyCollectionChangedEventHandler WordsChanged;
        public virtual event NotifyCollectionChangedEventHandler WordTemplatesChanged;
        private bool _custom;
        private Models.Doctor _doc;
        private string p;

        public virtual string Title
        {
            get { return _title; }
            set
            {
                SetProperty(ref _title, value ?? "", () => Title);
            }
        }
        public virtual bool IsCustom
        {
            get { return Doctor != null; }
            set
            {
                SetProperty(ref _custom, value, () => IsCustom);
            }
        }
        public virtual Doctor Doctor
        {
            get { return _doc; }
            set
            {
                SetProperty(ref _doc, value, () => Doctor);
            }
        }

        public virtual IEnumerable<WordTemplate> WordTemplates
        {
            get { return wordTemplates; }
        }
        public virtual IEnumerable<Speciality> Specialities
        {
            get { return specialities; }
        }
        public virtual IEnumerable<Word> Words
        {
            get { return words; }
        }

        public virtual Word AddWord(Word w)
        {
            if (!words.Contains(w))
            {
                words.Add(w);
                w.AddVoc(this);

                OnWordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, w));
            }
            return w;
        }

        public virtual void RemoveWord(Word w)
        {
            if (words.Remove(w))
            {
                w.RemoveVoc(this);
                OnWordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, w));
            }
        }
        internal protected virtual WordTemplate AddWordTemplate(WordTemplate wt)
        {
            Contract.Requires(wt.Vocabulary == this);
            if (!wordTemplates.Contains(wt))
            {
                wordTemplates.Add(wt);
                OnWordTemplatesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, wt));
            }
            return wt;
        }
        internal protected virtual void RemoveWordTemplate(WordTemplate wt)
        {
            Contract.Requires(wt.Vocabulary == this);
            if (wordTemplates.Remove(wt))
            {
                OnWordTemplatesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, wt));
            }
        }

        public override string ToString()
        {
            return Title;
        }

        protected virtual void OnWordsChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = WordsChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
        protected virtual void OnWordTemplatesChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = WordTemplatesChanged;
            if (h != null)
            {
                h(this, e);
            }
        }

        public override ValidationResult SelfValidate()
        {
            return new VocabularyValidator().Validate(this);
        }
    }
}