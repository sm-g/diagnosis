using System;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class WordTemplate : EntityBase<Guid>
    {
        private string _title;
        private Vocabulary _voc;

        public WordTemplate(string title, Vocabulary voc)
        {
            Contract.Requires(title != null);
            Contract.Requires(voc != null);

            Title = title;
            _voc = voc;
            _voc.AddWordTemplate(this);
        }

        protected WordTemplate()
        {
        }

        public virtual string Title
        {
            get { return _title; }
            set
            {
                SetProperty(ref _title, value ?? "", () => Title);
            }
        }

        public virtual Vocabulary Vocabulary
        {
            get { return _voc; }
            set
            {
                SetProperty(ref _voc, value, () => Vocabulary);
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}