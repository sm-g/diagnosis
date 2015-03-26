using Diagnosis.Models.Validators;
using FluentValidation.Results;
using Iesi.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class WordTemplate : EntityBase<Guid>
    {
        private string _title;
        private Vocabulary _voc;
        protected WordTemplate()
        {
        }

        public virtual string Title
        {
            get { return _title; }
            set
            {
                Contract.Requires(!String.IsNullOrEmpty(value));
                SetProperty(ref _title, value, () => Title);
            }
        }

        public virtual Vocabulary Vocabulary
        {
            get { return _voc; }
            set { SetProperty(ref _voc, value, () => Vocabulary); }
        }

        public override string ToString()
        {
            return Title;
        }


    }
}