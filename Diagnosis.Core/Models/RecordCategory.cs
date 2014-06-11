using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;

namespace Diagnosis.Models
{
    public class Category
    {
        public virtual int Id { get; protected set; }
        public virtual string Title { get; set; }
        public virtual int Order { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
