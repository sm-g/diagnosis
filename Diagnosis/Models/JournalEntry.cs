using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class JournalEntry
    {
        public DateTime Date { get; set; }
        public Symptom Symptom { get; set; }
        public Patient Patient { get; set; }
    }
}
