using System;
using System.Diagnostics.Contracts;
using Diagnosis.Common;

namespace Diagnosis.Models
{
    public class WordTemplate : EntityBase<Guid>
    {
        public const int MaxLength = 100;

        private string _title;
        private Vocabulary _voc;

        public WordTemplate(string title, Vocabulary voc)
        {
            Contract.Requires(!title.IsNullOrEmpty());
            Contract.Requires(voc != null);

            Title = title;
            Vocabulary = voc;
        }

        protected WordTemplate()
        {
        }

        public virtual string Title
        {
            get { return _title; }
            protected internal set
            {
                SetProperty(ref _title, value.TrimedOrNull() ?? "", () => Title);
            }
        }
        /// <summary>
        /// Словарь, для которого создан шаблон.
        /// </summary>
        public virtual Vocabulary Vocabulary
        {
            get { return _voc; }
            protected set
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