using System.Collections.Generic;
using System.Collections.ObjectModel;
using Iesi.Collections.Generic;
using System;

namespace Diagnosis.Models
{
    public class IcdDisease : EntityBase<int>, IDomainObject, IHrItemObject, IComparable<IcdDisease>, IIcdEntity
    {
        Iesi.Collections.Generic.ISet<HealthRecord> healthRecords;

        public virtual IcdBlock IcdBlock { get; protected set; }
        public virtual string Title { get; protected set; }
        public virtual string Code { get; protected set; }
        public virtual IEnumerable<HealthRecord> HealthRecords
        {
            get
            {
                return healthRecords;
            }
        }

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
