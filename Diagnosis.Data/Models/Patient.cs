using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class Patient : Man
    {
        public virtual int Id { get; protected set; }
        public virtual DateTime BirthDate { get; set; }
        public virtual int SNILS { get; set; }

        public Patient()
        {
            BirthDate = new DateTime(1980, 6, 15);
            IsMale = true;
        }
    }
}
