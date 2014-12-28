using System.Collections.Generic;
using System.Collections.ObjectModel;
using Iesi.Collections.Generic;
using System;

namespace Diagnosis.Models
{
    [Serializable]
    public class IcdDisease : EntityBase<int>, IDomainObject, IHrItemObject, IComparable<IcdDisease>, IIcdEntity
    {
        [NonSerialized]
        IcdBlock _icdBlock;
        [NonSerialized]
        private string _title;

        public virtual IcdBlock IcdBlock { get { return _icdBlock; } protected set { _icdBlock = value; } }
        public virtual string Title { get { return _title; } protected set { _title = value; } }
        public virtual string Code { get; protected set; }

        protected IcdDisease() { }

        public virtual int CompareTo(IHrItemObject hio)
        {
            var icd = hio as IcdDisease;
            if (icd != null)
                return this.CompareTo(icd);

            return -1; // 'smallest'
        }
        public virtual int CompareTo(IcdDisease other)
        {
            return this.Code.CompareTo(other.Code);
        }

        IIcdEntity IIcdEntity.Parent
        {
            get { return IcdBlock; }
        }

        public override string ToString()
        {
            return Code;
        }
    }
}
