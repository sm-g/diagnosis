using Diagnosis.Common;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    [Serializable]
    public class Comment : IHrItemObject, IComparable<Comment>
    {
        private string _text;

        public Comment(string text)
        {
            Contract.Requires(text != null);

            String = text;
        }

        protected Comment()
        {
        }

        public virtual string String
        {
            get { return _text; }
            set
            {
                _text = value.Prettify().Truncate(Length.Comment);
            }
        }

        public override string ToString()
        {
            return String;
        }

        public virtual int CompareTo(IHrItemObject hio)
        {
            var comment = hio as Comment;
            if (comment != null)
                return this.CompareTo(comment);
            else
                return new HrItemObjectComparer().Compare(this, hio);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Comment;
            if (other == null)
                return false;
            else
                return StringComparer.OrdinalIgnoreCase.Equals(other.String, this.String);
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