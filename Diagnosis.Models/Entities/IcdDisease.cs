using System;

namespace Diagnosis.Models
{
    [Serializable]
    public class IcdDisease : EntityBase<int>, IDomainObject, IHrItemObject, IComparable<IcdDisease>, IIcdEntity
    {
        [NonSerialized]
        private IcdBlock _icdBlock;

        [NonSerialized]
        private string _title;

        protected IcdDisease()
        {
        }

        public virtual IcdBlock IcdBlock { get { return _icdBlock; } protected set { _icdBlock = value; } }

        public virtual string Title { get { return _title; } protected set { _title = value; } }

        public virtual string Code { get; protected set; }

        /// <summary>
        /// Болезнь из подрубрики, например A01.2
        /// </summary>
        public virtual bool IsSubdivision { get { return Code.IndexOf('.') > 0; } }

        /// <summary>
        /// Трехзначный код рубрики, А01
        /// </summary>
        public virtual string DivisionCode { get { return Code.Substring(0, 3); } }

        IIcdEntity IIcdEntity.Parent
        {
            get { return IcdBlock; }
        }

        public virtual int CompareTo(IHrItemObject hio)
        {
            var icd = hio as IcdDisease;
            if (icd != null)
                return this.CompareTo(icd);
            else
                return new HrItemObjectComparer().Compare(this, hio);
        }

        public virtual int CompareTo(IcdDisease other)
        {
            if (other == null)
                return -1;
            return this.Code.CompareTo(other.Code);
        }

        public virtual int CompareTo(IIcdEntity other)
        {
            if (other == null)
                return -1;
            return this.Code.CompareTo(other.Code);
        }

        public override string ToString()
        {
            return Code;
        }
    }
}