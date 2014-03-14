using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class Course
    {
        public Patient Patient { get; set; }
        public Doctor LeadDoctor { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
    }
}
