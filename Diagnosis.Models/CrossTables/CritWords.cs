﻿using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class CritWords : EntityBase<Guid>
    {
        public CritWords(CritBase cr, Word w)
        {
            Contract.Requires(cr != null);
            Contract.Requires(w != null);

            Crit = cr;
            Word = w;
        }
        protected CritWords()
        {
        }

        public virtual CritBase Crit { get; set; }

        public virtual Word Word { get; set; }
    }
}