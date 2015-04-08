using Diagnosis.Models.Validators;
using FluentValidation.Results;
using Iesi.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class Vocabulary : ValidatableEntity<Guid>, IDomainObject
    {
        public static string CustomTitle = "Пользовательский";

        private Iesi.Collections.Generic.ISet<WordTemplate> wordTemplates = new HashedSet<WordTemplate>();
        private Iesi.Collections.Generic.ISet<SpecialityVocabularies> specialityVocabularies = new HashedSet<SpecialityVocabularies>();
        private Iesi.Collections.Generic.ISet<VocabularyWords> vocabularyWords = new HashedSet<VocabularyWords>();
        private string _title;
        private Doctor _doc;

        public Vocabulary(string title, Doctor d = null)
        {
            Contract.Requires(title != null);

            Title = title;
            if (d != null)
            {
                Doctor = d;
            }
        }

        protected Vocabulary()
        {
        }

        public virtual event NotifyCollectionChangedEventHandler WordsChanged;
        public virtual event NotifyCollectionChangedEventHandler SpecialitiesChanged;

        public virtual event NotifyCollectionChangedEventHandler WordTemplatesChanged;
        public virtual IEnumerable<SpecialityVocabularies> SpecialityVocabularies
        {
            get { return specialityVocabularies; }
        }
        public virtual IEnumerable<VocabularyWords> VocabularyWords
        {
            get { return vocabularyWords; }
        }
        public virtual string Title
        {
            get { return _title; }
            set
            {
                SetProperty(ref _title, value ?? "", () => Title);
            }
        }

        /// <summary>
        /// Если словарь пользовательский
        /// </summary>
        public virtual Doctor Doctor
        {
            get { return _doc; }
            set
            {
                if (SetProperty(ref _doc, value, () => Doctor))
                    OnPropertyChanged(() => IsCustom);
            }
        }

        public virtual bool IsCustom
        {
            get { return Doctor != null; }
        }

        public virtual IEnumerable<WordTemplate> WordTemplates
        {
            get { return wordTemplates; }
        }

        public virtual IEnumerable<Speciality> Specialities
        {
            get
            {
                return specialityVocabularies
                    .Where(x => x.Vocabulary == this)
                    .Select(x => x.Speciality);
            }
        }

        public virtual IEnumerable<Word> Words
        {
            get
            {
                return vocabularyWords
                    .Where(x => x.Vocabulary == this)
                    .Select(x => x.Word);
            }
        }

        public virtual Word AddWord(Word w)
        {
            if (!Words.Contains(w))
            {
                var vw = new VocabularyWords(this, w);
                vocabularyWords.Add(vw);
                w.AddVoc(this);

                OnWordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, w));
            }
            return w;
        }

        public virtual void RemoveWord(Word w)
        {
            var vw = vocabularyWords.Where(x => x.Word == w).FirstOrDefault();
            if (vw != null)
            {
                vocabularyWords.Remove(vw);
                w.RemoveVoc(this);

                OnWordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, w));
            }
        }

        protected internal virtual Speciality AddSpec(Speciality spec)
        {
            Contract.Requires(spec.Vocabularies.Contains(this));
            if (!Specialities.Contains(spec))
            {
                var sv = spec.SpecialityVocabularies.Where(x => x.Vocabulary == this).FirstOrDefault();
                specialityVocabularies.Add(sv);

                OnSpecialitiesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, spec));
            }
            return spec;
        }

        protected internal virtual void RemoveSpec(Speciality spec)
        {
            Contract.Requires(!spec.Vocabularies.Contains(this));
            var sv = specialityVocabularies.Where(x => x.Speciality == spec).FirstOrDefault();
            if (sv != null)
            {
                specialityVocabularies.Remove(sv);
                OnSpecialitiesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, spec));
            }
        }
        public virtual IEnumerable<string> GetOrderedTemplateTitles()
        {
            return WordTemplates.Select(x => x.Title).OrderBy(x => x);
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
        protected virtual void OnSpecialitiesChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = SpecialitiesChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
        public override ValidationResult SelfValidate()
        {
            return new VocabularyValidator().Validate(this);
        }

        public virtual void AddTemplates(IEnumerable<string> templatesToAdd)
        {
            Contract.Requires(templatesToAdd != null);

            SetTemplates(wordTemplates.Select(x => x.Title).Union(templatesToAdd));
        }

        public virtual void SetTemplates(IEnumerable<string> titlesToBe)
        {
            Contract.Requires(titlesToBe != null);

            var wasTitles = this.WordTemplates.Select(x => x.Title).ToList();

            // сохраняем регистр слов, но заменяем при смене регистра

            var toAdd = new HashSet<string>(titlesToBe);
            toAdd.ExceptWith(wasTitles);

            var toRemove = new HashSet<string>(wasTitles);
            toRemove.ExceptWith(titlesToBe);

            var templatesToRem = new List<WordTemplate>();
            var templatesToAdd = new List<WordTemplate>();

            // убираем ненужные
            foreach (var item in toRemove)
            {
                var old = wordTemplates.FirstOrDefault(x => x.Title == item);
                templatesToRem.Add(old);
            }

            // добавляем новые
            foreach (var item in toAdd)
            {
                var n = new WordTemplate(item, this);
                templatesToAdd.Add(n);
            }

            foreach (var item in templatesToRem)
            {
                wordTemplates.Remove(item);
            }
            foreach (var item in templatesToAdd)
            {
                wordTemplates.Add(item);
            }

            if (templatesToRem.Count > 0 || templatesToAdd.Count > 0)
            {
                OnWordTemplatesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}