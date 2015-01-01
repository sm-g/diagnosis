using System;

namespace Diagnosis.Models
{
    [Serializable]
    public class Measure : IDomainObject, IHrItemObject, IComparable<Measure>
    {
        public const short Scale = 6;
        public const short Precision = 18;

        private Uom _uom;

        public virtual Word Word { get; set; }

        public virtual Uom Uom
        {
            get { return _uom; }
            set
            {
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

        public Measure(double value, Uom uom = null)
        {
            Uom = uom;
            Value = value;
        }

        protected Measure()
        {
        }

        public override string ToString()
        {
            return string.Format("{0} {1}{2}", Word, Value, Uom != null ? " " + Uom : "");
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
            // сравниваем по словам
            if (this.Word != null)
            {
                if (other.Word == null)
                    return 1;
                else
                {
                    var byWord = this.Word.CompareTo(other.Word);
                    if (byWord != 0)
                        return byWord;
                }
            }
            // по типу
            if (this.Uom != null && other.Uom != null
                && this.Uom.Type != other.Uom.Type)
                return this.Uom.Type.CompareTo(other.Uom.Type);

            // по значению
            return this.DbValue.CompareTo(other.DbValue);
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
                return this.Word == other.Word && this.Uom == other.Uom && this.DbValue == other.DbValue;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode(); // TODO
        }

        public static bool operator ==(Measure x, Measure y)
        {
            return Object.Equals(x, y);
        }

        public static bool operator !=(Measure x, Measure y)
        {
            return !(x == y);
        }
    }
}