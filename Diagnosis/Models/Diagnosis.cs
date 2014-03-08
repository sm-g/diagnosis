using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Diagnosis
    {
        public string Title { get; set; }
        public string Code { get; set; }
        public Diagnosis Parent { get; set; }

        public Diagnosis(string title)
        {
            Contract.Requires(title != null);
            Contract.Requires(title.Length > 0);
            Title = title;
        }

        public Diagnosis() { }
    }
}
