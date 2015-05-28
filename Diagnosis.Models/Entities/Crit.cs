using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Models
{
    public abstract class Crit : ValidatableEntity<Guid>, IDomainObject, ICrit
    {
        private string _description;
        private Many2ManyHelper<CritWords, Word> crwHelper;
        private ISet<CritWords> critWords = new HashSet<CritWords>();

        public Crit()
        {
            Description = "";
        }

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
    }
}