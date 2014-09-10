using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Speciality : EntityBase
    {
        ISet<IcdBlock> icdBlocks;
        ISet<Doctor> doctors;
        ISet<SpecialityIcdBlock> specialityIcdBlocks;


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

        public virtual IEnumerable<SpecialityIcdBlock> SpecialityIcdBlocks
        {
            get { return new List<SpecialityIcdBlock>(specialityIcdBlocks); }
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
