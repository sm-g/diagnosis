using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    [Serializable]
    public class HealthRecordEventArgs : EventArgs
    {
        public readonly HealthRecord hr;

        [System.Diagnostics.DebuggerStepThrough]
        public HealthRecordEventArgs(HealthRecord hr)
        {
            this.hr = hr;
        }
    }
    [Serializable]
    public class DomainEntityEventArgs : EventArgs
    {
        public readonly IDomainObject entity;

        [System.Diagnostics.DebuggerStepThrough]
        public DomainEntityEventArgs(IDomainObject entity)
        {
            this.entity = entity;
        }
    }
    [Serializable]
    public class HrsHolderEventArgs : EventArgs
    {
        public readonly IHrsHolder holder;

        [System.Diagnostics.DebuggerStepThrough]
        public HrsHolderEventArgs(IHrsHolder holder)
        {
            this.holder = holder;
        }
    }


    [Serializable]
    public class UserEventArgs : EventArgs
    {
        public readonly IUser user;

        [System.Diagnostics.DebuggerStepThrough]
        public UserEventArgs(IUser user)
        {
            this.user = user;
        }
    }
}
