using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Diagnosis.Common;
using System.Diagnostics.Contracts;

namespace Diagnosis.Models
{
    [Serializable]
    public class Measure : IDomainObject, IHrItemObject, IComparable<Measure>
    {
        public const short Scale = 6;

        private Uom _uom;

        public Measure(double value, Uom uom = null, Word word = null)
        {
            Uom = uom;
            Value = value;
            Word = word;
        }

        protected Measure()
        {
        }

        public virtual Word Word { get; set; }

        public virtual Uom Uom
        {
            get { return _uom; }
            set
            {
                if (value == _uom) return;

                if (value == Uom.Null) value = null;

                // save value after chanage uom
                var val = Value;
                _uom = value;
                Value = val;
            }
        }

        /// <summary>
        /// Значение измерения, приведенное к базовой единице измерения.
        /// </summary>
        public virtual double DbValue { get; protected set; }

        /// <summary>
        /// Значение измерения. Если не указана единица, Value == DbValue.
        /// </summary>
        public virtual double Value
        {
            get
            {
                return Math.Round(
                    DbValue * (Uom != null ? (double)Math.Pow(10, -Uom.Factor) : 1),
                    Uom != null && (Uom.Factor % 1) == 0 ? Scale : 3); // для времени с дробным фактором округляем до 3
            }
            set
            {
                DbValue = value * (Uom != null ? (double)Math.Pow(10, Uom.Factor) : 1);
            }
        }

        public static bool operator ==(Measure x, Measure y)
        {
            return Object.Equals(x, y);
        }

        public static bool operator !=(Measure x, Measure y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}{2}", Word, Value, Uom != null ? "\u00A0" + Uom.Abbr.Replace(" ", "\u00A0") : ""); // nbsp in and before abbr
        }

        public virtual int CompareTo(IHrItemObject hio)
        {
            var measure = hio as Measure;
            if (measure != null)
                return this.CompareTo(measure);
            else
                return new HrItemObjectComparer().Compare(this, hio);
        }
        /// <summary>
        /// Сравнивает измерения учитывая единицу измерения (для OrderedBag).
        /// </summary>
        /// <param name="hio"></param>
        /// <returns></returns>
        public virtual int StrictCompareTo(IHrItemObject hio)
        {
            var measure = hio as Measure;
            if (measure != null)
            {
                var comp = this.CompareTo(measure);
                if (comp == 0 && this.Uom != null && measure.Uom != null)
                    // значение одинково, но единицы (одного типа) разные
                    return this.Uom.Abbr.CompareTo(measure.Uom.Abbr);
                return comp;
            }
            else
                return new HrItemObjectComparer().Compare(this, hio);
        }

        public virtual int CompareTo(Measure other)
        {
            if (other == null) return 1;
            if (this == other) return 0;

            // сравниваем по словам - больше измерение со словом
            var byWord = CompareByWord(other);
            if (byWord.HasValue && byWord != 0)
                return byWord.Value;

            // по типу единицы - если разный тип, больше измерение с типом, большим по порядку
            var byUom = CompareByUom(other);
            if (byUom.HasValue && byUom != 0)
                return byUom.Value;

            // одинаковые слова и тип единицы - по значению
            var byDbVal = this.DbValue.CompareTo(other.DbValue);
            return byDbVal;
        }

        /// <summary>
        /// Больше измерение с единицей.
        /// Если единицы у обоих, и разный тип единицы, больше измерение с типом, большим по порядку
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        internal int? CompareByUom(Measure other)
        {
            if (this.Uom == null && other.Uom != null)
                return -1;
            if (this.Uom != null && other.Uom == null)
                return 1;
            if (this.Uom != null && other.Uom != null)
                if (this.Uom.Type != other.Uom.Type)
                    return this.Uom.Type.CompareTo(other.Uom.Type);
                else
                    return 0;

            // нет единиц, нельзя сравнить
            return null;
        }

        /// <summary>
        /// Больше измерение со словом
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        internal int? CompareByWord(Measure other)
        {
            if (this.Word == null && other.Word != null)
                return -1;
            if (this.Word != null && other.Word == null)
                return 1;
            if (this.Word != null && other.Word != null)
                return this.Word.CompareTo(other.Word);

            // нет слов, нельзя сравнить
            return null;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Measure;
            if (other == null)
            {
                return false;
            }
            else
            {
                // не равны, даже если одно значение выражено разными единицами
                // 1 л != 1000 мл
                return this.Word == other.Word && this.Uom == other.Uom && this.DbValue == other.DbValue;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                if (Word != null)
                    hash = hash * 23 + Word.GetHashCode();
                if (Uom != null)
                    hash = hash * 23 + Uom.GetHashCode();
                hash = hash * 23 + DbValue.GetHashCode();
                return hash;
            }
        }
    }

    [Serializable]
    public class MeasureOp : Measure
    {
        private MeasureOperator op;
        double leftValue;

        public MeasureOp(MeasureOperator op, double value, Uom uom = null, Word word = null)
            : base(value, uom, word)
        {
            Operator = op;
            if (value < LeftBetweenValue)
            {
                LeftBetweenValue--;
            }
        }

        public MeasureOperator Operator
        {
            get { return op; }
            set { op = value; }
        }

        public double LeftBetweenValue
        {
            get { return leftValue; }
            set
            {
                if (value > DbValue)
                {
                    var x = DbValue;
                    DbValue = value;
                    leftValue = x;
                }
                else
                {
                    leftValue = value;
                }
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

                    var byLeftDbVal = m.DbValue.CompareTo(this.LeftBetweenValue);
                    var byRightDbVal = m.DbValue.CompareTo(this.DbValue);
                    return byLeftDbVal >= 0 &&
                           byRightDbVal <= 0;

                default:
                    throw new NotImplementedException();
            }
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(LeftBetweenValue <= DbValue);
        }

        public override string ToString()
        {
            string operatorString = AttributesHelper.GetEnumDescription(Operator);

            return string.Format("{0} {1} {2}{3}", Word, operatorString, Value, Uom != null ? "\u00A0" + Uom.Abbr.Replace(" ", "\u00A0") : ""); // nbsp in and before abbr
        }
    }
    /// <summary>
    /// usage:
    /// mOp = new MeasureOp(5, MeasureOperator.Equal)
    /// 
    /// есть измерения, равные 5
    /// Measures.Contains(mOp, new MeasureValueComparer(mOp.Operator))
    /// 
    /// m == m2 по значению
    /// new MeasureValueComparer(MeasureOperator.Equal).Equals(m, m2)
    /// </summary>
    public class MeasureValueComparer : IEqualityComparer<Measure>
    {
        private MeasureOperator op;

        public MeasureValueComparer(MeasureOperator op)
        {
            this.op = op;
        }

        public bool Equals(Measure x, Measure y)
        {
            switch (op)
            {
                case MeasureOperator.GreaterOrEqual:
                    return x.CompareTo(y) >= 0;

                case MeasureOperator.Greater:
                    return x.CompareTo(y) > 0;

                case MeasureOperator.Equal:
                    return x.CompareTo(y) == 0;

                case MeasureOperator.Less:
                    return x.CompareTo(y) < 0;

                case MeasureOperator.LessOrEqual:
                    return x.CompareTo(y) <= 0;

                case MeasureOperator.Between:
                default:
                    throw new NotImplementedException();
            }
        }

        public int GetHashCode(Measure obj)
        {
            // разный хеш - equals not called
            return 0;
        }
    }
}