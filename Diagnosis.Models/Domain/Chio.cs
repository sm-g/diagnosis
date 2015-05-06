using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    /// <summary>
    /// Сущность элемента записи с уверенностью.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("CHIO {HIO}{HioType} {Confidence}")]
    public class ConfWithHio : IDomainObject, IComparable<ConfWithHio>
    {
        public Confidence Confidence { get; set; }

        public IHrItemObject HIO { get; set; }

        public ConfWithHio(IHrItemObject hio, Confidence conf = Models.Confidence.Present)
        {
            Contract.Requires(hio != null);

            Confidence = conf;
            HIO = hio;
        }

        public int CompareTo(ConfWithHio other)
        {
            // сравниваем строго, так что измерения с одним значением, 
            // выраженным разными единицами, не равны
            var res = StrictIHrItemObjectComparer.StrictCompare(this.HIO, other.HIO);
            if (res != 0) return res;

            return this.Confidence.CompareTo(other.Confidence);
        }

        public override bool Equals(object obj)
        {
            var other = obj as ConfWithHio;
            if (other == null)
                return false;

            return
                this.Confidence.Equals(other.Confidence) &&
                this.HIO.Equals(other.HIO);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return HIO.GetHashCode() * 37 + Confidence.GetHashCode();
            }
        }

        public override string ToString()
        {
            string suf;
            switch (Confidence)
            {
                case Confidence.Notsure:
                    suf = " (не уверен)";
                    break;
                case Confidence.Absent:
                    suf = " (нет)";
                    break;
                default:
                case Confidence.Present:
                    suf = "";
                    break;
            }
            return string.Format("{0}{1}", HIO, suf);
        }

        protected Type HioType { get { return HIO.GetType(); } }

        public int CompareTo(IHrItemObject other)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    [DebuggerDisplay("CHIO {HIO}{HioType} {Confidence}")]
    public class Confindencable<T> : ConfWithHio where T : IHrItemObject
    {
        public new T HIO { get; set; }

        public Confindencable(T hio, Confidence conf = Models.Confidence.Present)
            : base(hio, conf)
        {
            Confidence = conf;
            HIO = hio;
        }
    }

}
