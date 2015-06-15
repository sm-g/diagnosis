using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public abstract class CritBase : ValidatableEntity<Guid>, IDomainObject, ICrit
    {
        private string _description;
        private Many2ManyHelper<CritWords, Word> crwHelper;
        private ISet<CritWords> critWords = new HashSet<CritWords>();
        private string _options;
        private string _optionsFormat;

        public CritBase()
        {
            Description = "";
        }

        public virtual event NotifyCollectionChangedEventHandler WordsChanged;

        public virtual string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value.Truncate(Length.CritDescr), () => Description); }
        }

        public virtual IEnumerable<Word> Words
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

        public virtual string Options
        {
            get { return _options; }
            set { SetProperty(ref _options, value, () => Options); }
        }
        /// <summary>
        /// Сериализатор опций.
        /// </summary>
        public virtual string OptionsFormat
        {
            get { return _optionsFormat; }
            set { SetProperty(ref _optionsFormat, value, () => OptionsFormat); }
        }

        private Many2ManyHelper<CritWords, Word> CrwHelper
        {
            get
            {
                if (crwHelper == null)
                {
                    crwHelper = new Many2ManyHelper<CritWords, Word>(
                        critWords,
                        x => x.Crit == this,
                        x => x.Word);
                }
                return crwHelper;
            }
        }

        public virtual void SetWords(IEnumerable<Word> wordsToBe)
        {
            Contract.Requires(wordsToBe != null);
            Contract.Ensures(Words.ScrambledEquals(wordsToBe));

            var was = this.Words.ToList();
            wordsToBe = wordsToBe.Distinct();

            foreach (var item in was.Except(wordsToBe).ToList())
            {
                RemoveWord(item);
            }
            foreach (var item in wordsToBe.Except(was).ToList())
            {
                AddWord(item);
            }
        }

        public virtual Word AddWord(Word w)
        {
            if (!Words.Contains(w))
            {
                var vw = new CritWords(this, w);
                CrwHelper.Add(vw);

                w.AddCritWords(vw);

                OnWordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, w));
            }
            return w;
        }

        public virtual void RemoveWord(Word w)
        {
            if (CrwHelper.Remove(w))
            {
                w.RemoveCrit(this);

                OnWordsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, w));
            }
        }

        protected virtual void OnWordsChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = WordsChanged;
            if (h != null)
            {
                h(this, e);
            }
        }

        public abstract bool IsEmpty();
    }
}