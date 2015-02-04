using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    public class Diagnosis : IDomainObject
    {
        public Diagnosis(string code, string title, Diagnosis parent = null, IcdDisease disease = null)
        {
            Contract.Requires(!string.IsNullOrEmpty(title));
            Contract.Requires(code != null);
            Contract.Requires(code.Length <= 10);

            Code = code;
            Title = title;
            Parent = parent;
            Disease = disease;
        }

        public virtual string Title { get; set; }

        public virtual string Code { get; set; }

        public virtual IcdDisease Disease { get; set; }

        public virtual Diagnosis Parent { get; set; }
        public override string ToString()
        {
            return Code + ' ' + Title;
        }
    }
}