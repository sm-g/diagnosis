using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public abstract class Crit : ValidatableEntity<Guid>, IDomainObject, ICrit
    {
        private string _description;
        private Many2ManyHelper<CritWords, Word> crwHelper;
        private ISet<CritWords> critWords = new HashSet<CritWords>();

        private string _options;

        public Crit()
        {
            Description = "";
        }

        public virtual event NotifyCollectionChangedEventHandler WordsChanged;

        public virtual string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value.Truncate(Length.CriterionDescr), () => Description); }
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

        public virtual void ReplaceWord(Word word, string oldTitle)
        {
            Contract.Requires(word != null);
            Contract.Requires(Words.Contains(word));

            // меняем текст прямо в сериализованной строке запроса
            Options = Options.Replace(oldTitle, word.Title);
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
    }
}