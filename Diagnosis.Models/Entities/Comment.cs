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
    [Serializable]
    public class Comment : IHrItemObject, IComparable<Comment>
    {
        public virtual string String { get; set; }

        public Comment(string text)
        {
            Contract.Requires(text != null);

            String = text;
        }

        protected Comment() { }

        public override string ToString()
        {
            return String;
        }
        public virtual int CompareTo(IHrItemObject hio)
        {
            var comment = hio as Comment;
            if (comment != null)
                return this.CompareTo(comment);

            var word = hio as Word;
            if (word != null)
                return -1;

            return 1;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Comment;
            if (other == null)
                return false;
            else
                return String == other.String;
        }

        public override int GetHashCode()
        {
            return String.GetHashCode();
        }

        public virtual int CompareTo(Comment other)
        {
            return this.String.CompareTo(other.String);
        }
    }
}
