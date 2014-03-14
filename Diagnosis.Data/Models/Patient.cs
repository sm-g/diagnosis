using System;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class Patient
    {
        public virtual int Id { get; protected set; }
        public virtual string FirstName { get; set; }
        public virtual string MiddleName { get; set; }
        public virtual string LastName { get; set; }
        public virtual bool IsMale { get; set; }
        public virtual DateTime BirthDate { get; set; }
        public virtual string SNILS { get; set; }

        public Patient()
        {
            BirthDate = new DateTime(1980, 6, 15);
            IsMale = true;
        }
    }
}
