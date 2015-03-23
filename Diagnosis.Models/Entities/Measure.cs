using System;

namespace Diagnosis.Models
{
    [Serializable]
    public class Measure : IDomainObject, IHrItemObject, IComparable<Measure>
    {
        public const short Scale = 6;
        public const short Precision = 18;

        private Uom _uom;

        public Measure(double value, Uom uom = null)
        {
            Uom = uom;
            Value = value;
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
                return Math.Round(DbValue * (Uom != null ? (double)Math.Pow(10, -Uom.Factor) : 1), Scale);
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
            return string.Format("{0} {1}{2}", Word, Value, Uom != null ? " " + Uom.Abbr : "");
        }

        public virtual int CompareTo(IHrItemObject hio)
        {
            var icd = hio as IcdDisease;
            if (icd != null)
                return 1;

            var measure = hio as Measure;
            if (measure != null)
                return this.CompareTo(measure);

            return -1;
        }

        public virtual int CompareTo(Measure other)
        {
            if (other == null) return 1;
            if (this == other) return 0;

            // сравниваем по словам - больше измерение со словом
            if (this.Word == null && other.Word != null)
                return -1;
            if (this.Word != null && other.Word == null)
                return 1;
            else if (this.Word != null && other.Word != null)
            {
                var byWord = this.Word.CompareTo(other.Word);
                if (byWord != 0)
                    return byWord;
            }

            // по типу единицы - если разный тип, больше измерение с типом, большим по порядку
            if (this.Uom == null && other.Uom != null)
                return -1;
            if (this.Uom != null && other.Uom == null)
                return 1;
            else if (this.Uom != null && other.Uom != null)
                if (this.Uom.Type != other.Uom.Type)
                    return this.Uom.Type.CompareTo(other.Uom.Type);

            // одинаковые слова и тип единицы - по значению
            var byDbVal = this.DbValue.CompareTo(other.DbValue);
            if (byDbVal != 0)
                return byDbVal;

            // значение одинково, но единицы (одного типа) разные
            return this.Uom.Abbr.CompareTo(other.Uom.Abbr);
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
}