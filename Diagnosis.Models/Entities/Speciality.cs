using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class Speciality : EntityBase<int>, IDomainObject
    {
        IList<IcdBlock> icdBlocks;
        Iesi.Collections.Generic.ISet<Doctor> doctors;
        Iesi.Collections.Generic.ISet<SpecialityIcdBlocks> specialityIcdBlocks;


        public virtual string Title { get; protected set; }
        public virtual IEnumerable<IcdBlock> IcdBlocks
        {
            get
            {
                return icdBlocks;
            }
        }
        public virtual IEnumerable<Doctor> Doctors
        {
            get
            {
                return doctors;
            }
        }

        public virtual IEnumerable<SpecialityIcdBlocks> SpecialityIcdBlocks
        {
            get { return specialityIcdBlocks; }
        }

        public Speciality(string title)
        {
            Contract.Requires(!String.IsNullOrEmpty(title));

            Title = title;
        }

        protected Speciality() { }

        public override string ToString()
        {
            return Title;
        }
    }

}
