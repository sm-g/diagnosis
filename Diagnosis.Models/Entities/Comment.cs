using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class Comment : IHrItemObject
    {
        public virtual string String { get; set; }

        public Comment(string text)
        {
            String = text;
        }

        protected Comment() { }

        public override string ToString()
        {
            return String;
        }

    }
}
