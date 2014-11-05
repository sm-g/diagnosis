using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class Word : EntityBase, IDomainObject, IHrItemObject
    {
        private Iesi.Collections.Generic.ISet<Word> children;
        private string _title;

        public virtual string Title
        {
            get { return _title; }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                _title = value;
            }
        }

        public virtual HrCategory DefaultCategory { get; set; }

        public virtual Word Parent { get; set; }
        public virtual ObservableCollection<Word> Children
        {
            get
            {
                return new ObservableCollection<Word>(children);
            }
        }

        public Word(string title)
        {
            Contract.Requires(!string.IsNullOrEmpty(title));
            Title = title;
        }

        protected Word()
        {
        }

        public override string ToString()
        {
            return Title;
        }
    }
}