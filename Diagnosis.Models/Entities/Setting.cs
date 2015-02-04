using System;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Setting : EntityBase<Guid>
    {
        public Setting(Doctor doctor, string title)
        {
            Contract.Requires(doctor != null);
            Contract.Requires(!String.IsNullOrEmpty(title));

            Title = title;
            Doctor = doctor;
        }

        protected Setting()
        {
        }

        public virtual string Title { get; protected set; }

        public virtual string Value { get; set; }

        public virtual Doctor Doctor { get; protected set; }
        public override string ToString()
        {
            return Title;
        }
    }
}