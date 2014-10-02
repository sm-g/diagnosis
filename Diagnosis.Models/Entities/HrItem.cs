using Diagnosis.Core;
using Iesi.Collections.Generic;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class HrItem : EntityBase, IDomainEntity
    {

        private IcdDisease _disease;
        private Word _word;
        private Measure _measure;

        public HrItem(HealthRecord hr, Word word)
        {
            Contract.Requires(hr != null);

            HealthRecord = hr;
            Word = word;
            hr.AddItem(this);
        }

        public HrItem(HealthRecord hr, Measure measure)
        {
            Contract.Requires(hr != null);

            HealthRecord = hr;
            Measure = measure;
            hr.AddItem(this);
        }

        protected HrItem()
        {
        }

        public virtual HealthRecord HealthRecord { get; protected set; }
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

        public virtual IDomainEntity Entity
        {
            get
            {
                if (Word != null) return Word;
                if (Measure != null) return Measure;
                if (Disease != null) return Disease;

                return null;
            }
        }

        public virtual int Order { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ", Id);
        }

    }
}