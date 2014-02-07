using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class Patient
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public bool IsMale { get; set; }
        public bool OnlyBirthYear { get; set; }

        public Patient()
        {
            BirthDate = new DateTime(1980, 6, 15);
            IsMale = true;
        }
    }
}
