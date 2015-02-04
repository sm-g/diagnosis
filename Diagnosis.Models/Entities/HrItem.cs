using Diagnosis.Common;
using System;

using System;

using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class HrItem : EntityBase<Guid>, IDomainObject
    {
        private Comment comment;

        private IcdDisease _disease;
        private Word _word;
        private Measure _measure;

        public HrItem(HealthRecord hr, IHrItemObject obj)
        {
            Contract.Requires(hr != null);
            Contract.Requires(obj != null);

            HealthRecord = hr;

            if (obj is Word)
                Word = obj as Word;
            else if (obj is Measure)
            {
                Measure = obj as Measure;
                Word = Measure.Word;
            }
            else if (obj is IcdDisease)
                Disease = obj as IcdDisease;
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
                EditHelper.Edit<IcdDisease, Guid>(() => Disease);
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
                if (_measure != null && _word != null)
                {
                    _measure.Word = _word;
                }
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
                // measure and word in one Hri
                if (Measure != null) return Measure;
                if (Word != null) return Word;
                if (Disease != null) return Disease;
                if (TextRepr != null) return comment ?? (comment = new Comment(TextRepr));

                return null;
            }
        }

        public virtual int Ord { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", this.ShortId(), Ord, Entity);
        }
    }
}