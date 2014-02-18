using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class Symptom
    {
        public string Title { get; set; }
        public Symptom Parent { get; set; }
        public bool IsGroup { get; set; }

        public Symptom(string title)
        {
            Title = title;
        }

        public Symptom() { }
    }
}
