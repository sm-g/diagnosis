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
        private Comment _comment;
        private IcdDisease _disease;
        private Word _word;
        private Measure _measure;

        public HrItem(HealthRecord hr, IHrItemObject hio)
        {
            Contract.Requires(hr != null);
            Contract.Requires(hio != null);

            HealthRecord = hr;

            if (hio is Word)
                Word = hio as Word;
            else if (hio is Measure)
            {
                Measure = hio as Measure;
                Word = Measure.Word;
            }
            else if (hio is IcdDisease)
                Disease = hio as IcdDisease;
            else if (hio is Comment)
            {
                Comment = hio as Comment;
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

        public virtual IcdDisease Disease
        {
            get { return _disease; }
            protected set { SetProperty(ref _disease, value, () => Disease); }
        }

        public virtual Word Word
        {
            get { return _word; }
            protected set { SetProperty(ref _word, value, () => Word); }
        }
        public virtual Comment Comment
        {
            get { return _comment; }
            protected set { SetProperty(ref _comment, value, () => Comment); }
        }
        public virtual Measure Measure
        {
            get
            {
                if (_measure != null && _word != null)
                    _measure.Word = _word; // when get from db, set up Word

                return _measure;
            }
            protected set { SetProperty(ref _measure, value, () => Measure); }
        }

        public virtual IHrItemObject Entity
        {
            get
            {
                // measure and word in one Hri, first check measure
                if (Measure != null) return Measure;
                if (Word != null) return Word;
                if (Disease != null) return Disease;
                if (Comment != null) return Comment;

                return null; // still not initialized
            }
        }

        public virtual int Ord { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}{2}", this.ShortId(), Confidence == Models.Confidence.Absent ? "!" : "", Entity);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(Measure == null || Measure.Word == Word);
        }

    }


}