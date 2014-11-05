using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Diagnosis : IDomainEntity
    {
        public virtual string Title { get; set; }
        public virtual string Code { get; set; }
        public virtual IcdDisease Disease { get; set; }
        public virtual Diagnosis Parent { get; set; }


        public Diagnosis(string code, string title, Diagnosis parent = null, IcdDisease disease = null)
        {
            Contract.Requires(!string.IsNullOrEmpty(title));
            Contract.Requires(code != null && code.Length <= 10);

            Code = code;
            Title = title;
            Parent = parent;
            Disease = disease;
        }

        public override string ToString()
        {
            return Code + ' ' + Title;
        }
    }
}
