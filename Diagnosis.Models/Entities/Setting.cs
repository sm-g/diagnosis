using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class Setting : EntityBase<Guid>
    {
        public virtual string Title { get; protected set; }
        public virtual string Value { get; set; }

        public virtual Doctor Doctor { get; protected set; }


        public Setting(Doctor doctor, string title)
        {
            Contract.Requires(doctor != null);
            Contract.Requires(!String.IsNullOrEmpty(title));

            Title = title;
            Doctor = doctor;
        }
        protected Setting() { }

        public override string ToString()
        {
            return Title;
        }
    }

}
