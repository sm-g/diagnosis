using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Symptom
    {
        public string Title { get; set; }
        public int Level { get; set; }

        public Symptom(string title)
        {
            Contract.Requires(title != null);
            Contract.Requires(title.Length > 0);
            Title = title;
        }

        public Symptom() { }
    }
}
