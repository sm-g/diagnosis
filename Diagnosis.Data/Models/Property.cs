using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class Property
    {
        public string Title { get; set; }

        public Property(string title)
        {
            Title = title;
        }
    }
}
