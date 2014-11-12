using System.Collections.Generic;
using System.Collections.ObjectModel;
using Iesi.Collections.Generic;
using System;

namespace Diagnosis.Models
{
    public class IcdDisease : EntityBase, IDomainObject, IHrItemObject, IComparable<IcdDisease>
    {
        Iesi.Collections.Generic.ISet<HealthRecord> healthRecords;

        public virtual IcdBlock IcdBlock { get; protected set; }
        public virtual string Title { get; set; }
        public virtual string Code { get; set; }
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
    }
}
