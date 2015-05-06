using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    [Serializable]
    public class MeasureOp : Measure
    {
        private MeasureOperator op;
        private double andDbValue;

        public MeasureOp(MeasureOperator op, double value, Uom uom = null, Word word = null)
            : base(value, uom, word)
        {
            Operator = op;
        }

        public MeasureOperator Operator
        {
            get { return op; }
            set { op = value; }
        }

        public double RightValue
        {
            get
            {
                return DbValueToValue(RightDbValue);
            }
            set
            {
                RightDbValue = ValueToDbValue(value);
            }
        }

        public double RightDbValue
        {
            get { return andDbValue; }
            set
            {
                andDbValue = value;
            }
        }

        public virtual string FormattedRightValue
        {
            get
            {
                if (Uom == null)
                    return RightValue.ToString();

                return Uom.FormatValue(RightValue);
            }
        }

        /// <summary>
        /// m Operator this
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public bool ResultFor(Measure m)
        {
            if (m == null)
                return false;

            switch (Operator)
            {
                case MeasureOperator.GreaterOrEqual:
                    return m.CompareTo(this) >= 0;

                case MeasureOperator.Greater:
                    return m.CompareTo(this) > 0;

                case MeasureOperator.Equal:
                    return m.CompareTo(this) == 0;

                case MeasureOperator.Less:
                    return m.CompareTo(this) < 0;

                case MeasureOperator.LessOrEqual:
                    return m.CompareTo(this) <= 0;

                case MeasureOperator.Between:
                    var byWord = m.CompareByWord(this);
                    if (byWord.HasValue && byWord != 0)
                        return false;

                    var byUom = m.CompareByUom(this);
                    if (byUom.HasValue && byUom != 0)
                        return false;

                    // порядок не важен
                    var less = Math.Min(DbValue, RightDbValue);
                    var great = Math.Max(DbValue, RightDbValue);

                    if (less == great)
                        return m.DbValue == great;

                    return less < m.DbValue && // left < x <= right
                           m.DbValue <= great;

                default:
                    throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            string opValue;

            if (Operator.IsBinary())
                opValue = string.Format("{0}{1}{2}", FormattedValue, Operator.ToStr(), FormattedRightValue);
            else
                opValue = string.Format("{0} {1}", Operator.ToStr(), FormattedValue);

            return string.Format("{0}\u00A0{1}{2}", Word, opValue,
                Uom != null ? nbsp + Uom.Abbr.Replace(" ", nbsp) : ""); // nbsp in and before abbr
        }

        public override bool Equals(object obj)
        {
            var other = obj as MeasureOp;
            if (other == null) return false;

            return base.Equals(obj) && this.Operator == other.Operator && this.RightDbValue == other.RightDbValue;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = base.GetHashCode();
                hash = hash * 23 + Operator.GetHashCode();
                hash = hash * 23 + RightDbValue.GetHashCode();
                return hash;
            }
        }
    }
}
