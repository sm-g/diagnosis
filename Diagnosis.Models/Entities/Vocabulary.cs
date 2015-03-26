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
        private IList<Word> words = new List<Word>(); // many-2-many
        private string _title;

        public Vocabulary(string title)
        {
            Contract.Requires(title != null);

            Title = title;
        }

        protected Vocabulary()
        {
        }

        public virtual event NotifyCollectionChangedEventHandler WordsChanged;
        public virtual event NotifyCollectionChangedEventHandler WordTemplatesChanged;

        public virtual string Title
        {
            get { return _title; }
            set
            {
                Contract.Requires(!String.IsNullOrEmpty(value));
                SetProperty(ref _title, value, () => Title);
            }
        }

        public virtual IEnumerable<WordTemplate> WordTemplates
        {
            get { return wordTemplates; }
        }

        public virtual IEnumerable<Word> Words
        {
            get { return words; }
        }

        public virtual Word AddWord(Word w)
        {
            words.Add(w);
            OnWordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, w));
            return w;
        }

        public virtual void RemoveWord(Word w)
        {
            if (words.Remove(w))
                OnWordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, w));
        }

        public virtual void ClearWordTemplates()
        {
            wordTemplates.Clear();
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