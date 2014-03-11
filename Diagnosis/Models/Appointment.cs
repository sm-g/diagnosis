using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class Appointment
    {
        public Course Course { get; set; }
        public Doctor Doctor { get; set; }
        public DateTime DateTime { get; set; }
    }
}
