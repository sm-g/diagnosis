using Diagnosis.Common;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    /// <summary>
    /// Элемент записи. Value object, создается при изменении хранимой сущности, порядка или уверенности.
    /// </summary>
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
            else throw new NotImplementedException();
        }

        protected HrItem()
        {
        }

        public virtual HealthRecord HealthRecord { get; protected set; }

        /// <summary>
        /// Уверенность.
        /// </summary>
        public virtual Confidence Confidence { get; set; }

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

                return null; // still not initialized
            }
        }

        public virtual ConfindenceHrItemObject CHIO
        {
            get
            {
                return new ConfindenceHrItemObject(Entity, Confidence);
            }
        }

        public virtual int Ord { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", this.ShortId(), Ord, Entity);
        }
    }

    /// <summary>
    /// Сущность элемента записи с уверенностью.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("CHIO {HIO}{HioType} {Confidence}")]
    public class ConfindenceHrItemObject : IDomainObject, IComparable<ConfindenceHrItemObject>
    {
        public Confidence Confidence { get; set; }

        public IHrItemObject HIO { get; set; }

        public ConfindenceHrItemObject(IHrItemObject hio, Confidence conf)
        {
            Contract.Requires(hio != null);

            Confidence = conf;
            HIO = hio;
        }

        public int CompareTo(ConfindenceHrItemObject other)
        {
            // сравниваем строго, так что измерения с одним значением, 
            // выраженным разными единицами, не равны
            var res = StrictIHrItemObjectComparer.StrictCompare(this.HIO, other.HIO);
            if (res != 0) return res;

            return this.Confidence.CompareTo(other.Confidence);
        }

        public override bool Equals(object obj)
        {
            var other = obj as ConfindenceHrItemObject;
            if (other == null)
                return false;

            return
                this.Confidence.Equals(other.Confidence) &&
                this.HIO.Equals(other.HIO);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return HIO.GetHashCode() * 37 + Confidence.GetHashCode();
            }
        }

        public override string ToString()
        {
            string suf;
            switch (Confidence)
            {
                case Confidence.Notsure:
                    suf = " (не уверен)";
                    break;
                case Confidence.Absent:
                    suf = " (нет)";
                    break;
                default:
                case Confidence.Present:
                    suf = "";
                    break;
            }
            return string.Format("{0}{1}", HIO, suf);
        }

        private Type HioType { get { return HIO.GetType(); } }
    }
}