using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class HealthRecord
    {
        public Appointment Appointment { get; set; }
        public string Description { get; set; }
    }
}
