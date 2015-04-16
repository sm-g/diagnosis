using Diagnosis.Models.Validators;
using FluentValidation.Results;
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

        private ISet<WordTemplate> wordTemplates = new HashSet<WordTemplate>();
        private ISet<SpecialityVocabularies> specialityVocabularies = new HashSet<SpecialityVocabularies>();
        private ISet<VocabularyWords> vocabularyWords = new HashSet<VocabularyWords>();
        private string _title;
        private Doctor _doc;

        private Many2ManyHelper<VocabularyWords, Word> vwHelper;

        private Many2ManyHelper<Models.SpecialityVocabularies, Speciality> svHelper;
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
                return SvHelper.Values;
            }
        }

        public virtual IEnumerable<Word> Words
        {
            get
            {
                return VwHelper.Values;
            }
        }

        private Many2ManyHelper<Models.VocabularyWords, Word> VwHelper
        {
            get
            {
                if (vwHelper == null)
                {
                    vwHelper = new Many2ManyHelper<VocabularyWords, Word>(
                        vocabularyWords,
                        x => x.Vocabulary == this,
                        x => x.Word);
                }
                return vwHelper;
            }
        }
        private Many2ManyHelper<SpecialityVocabularies, Speciality> SvHelper
        {
            get
            {
                if (svHelper == null)
                {
                    svHelper = new Many2ManyHelper<SpecialityVocabularies, Speciality>(
                       specialityVocabularies,
                       x => x.Vocabulary == this,
                       x => x.Speciality);
                }
                return svHelper;
            }
        }
        public virtual Word AddWord(Word w)
        {
            if (!Words.Contains(w))
            {
                var vw = new VocabularyWords(this, w);
                VwHelper.Add(vw);

                w.AddVocWords(vw);

                OnWordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, w));
            }
            return w;
        }

        public virtual void RemoveWord(Word w)
        {
            if (VwHelper.Remove(w))
            {
                w.RemoveVoc(this);

                OnWordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, w));
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
            // шаблоны уникальны без учета регистра
            Contract.Ensures(WordTemplates.GroupBy(x => x.Title.ToLowerInvariant()).Count() == WordTemplates.Count());

            var wasTitles = this.WordTemplates.Select(x => x.Title).ToList();

            titlesToBe = titlesToBe.Distinct(
                StringComparer.CurrentCultureIgnoreCase);

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
        protected internal virtual void AddSpecVoc(SpecialityVocabularies sv)
        {
            Contract.Requires(sv.Speciality.Vocabularies.Contains(this));
            if (SvHelper.Add(sv))
                OnSpecialitiesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, sv));
        }

        protected internal virtual void RemoveSpec(Speciality spec)
        {
            Contract.Requires(!spec.Vocabularies.Contains(this));
            if (SvHelper.Remove(spec))
                OnSpecialitiesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, spec));
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
    }
}