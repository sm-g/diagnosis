using Diagnosis.Common;
using Iesi.Collections.Generic;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class HrItem : EntityBase, IDomainObject
    {
        private Comment comment;

        private IcdDisease _disease;
        private Word _word;
        private Measure _measure;

        public HrItem(HealthRecord hr, IHrItemObject obj)
        {
            Contract.Requires(hr != null);

            HealthRecord = hr;

            if (obj is Word)
                Word = obj as Word;
            else if (obj is Measure)
                Measure = obj as Measure;
            else if (obj is Comment)
            {
                comment = obj as Comment;
                TextRepr = (obj as Comment).String;
            }
        }

        protected HrItem()
        {
        }

        public virtual HealthRecord HealthRecord { get; protected set; }
        public virtual string TextRepr { get; protected set; }


        public virtual IcdDisease Disease
        {
            get { return _disease; }
            protected set
            {
                if (_disease == value)
                    return;
                EditHelper.Edit(() => Disease);
                _disease = value;
                OnPropertyChanged("Disease");
            }
        }
        public virtual Word Word
        {
            get
            {
                return _word;
            }
            protected set
            {
                if (_word != value)
                {
                    _word = value;
                    OnPropertyChanged(() => Word);
                }
            }
        }

        public virtual Measure Measure
        {
            get
            {
                return _measure;
            }
            protected set
            {
                if (_measure != value)
                {
                    _measure = value;
                    OnPropertyChanged(() => Measure);
                }
            }
        }

        public virtual IHrItemObject Entity
        {
            get
            {
                if (Word != null) return Word;
                if (Measure != null) return Measure;
                if (Disease != null) return Disease;
                if (TextRepr != null) return comment ?? (comment = new Comment(TextRepr));

                return null;
            }
        }

        public virtual int Ord { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Id, Ord, Entity);
        }

    }
}