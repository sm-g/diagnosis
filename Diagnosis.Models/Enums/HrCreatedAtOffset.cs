using System;
using System.Linq;

namespace Diagnosis.Models.Enums
{
    public struct HrCreatedAtOffset : IComparable
    {
        public HrCreatedAtOffset(DateTime createdAt)
            : this()
        {
            var span = (DateTime.Today - createdAt).Days;

            if (span < 1)
                Offset = CreatedOffset.Today;
            else if (span < 2)
                Offset = CreatedOffset.Yesterday;
            else if (span < 7)
                Offset = CreatedOffset.Week;
            else if (span < 30)
                Offset = CreatedOffset.Month;
            else
                Offset = CreatedOffset.LongAgo;
        }

        public enum CreatedOffset
        {
            // чтобы сегодня были внизу списка
            LongAgo,
            Month,
            Week,
            Yesterday,
            Today,
        }

        public CreatedOffset Offset { get; private set; }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is HrCreatedAtOffset))
                return 1;

            HrCreatedAtOffset other = (HrCreatedAtOffset)obj;
            return this.Offset.CompareTo(other.Offset);
        }

        public override string ToString()
        {
            string res;
            switch (Offset)
            {
                case CreatedOffset.Today:
                    res = "сегодня";
                    break;

                case CreatedOffset.Yesterday:
                    res = "вчера";
                    break;

                case CreatedOffset.Week:
                    res = "за неделю";
                    break;

                case CreatedOffset.Month:
                    res = "за последний месяц";
                    break;

                case CreatedOffset.LongAgo:
                default:
                    res = "давно";
                    break;
            }
            return res;
        }

        public static bool operator ==(HrCreatedAtOffset c1, HrCreatedAtOffset c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(HrCreatedAtOffset c1, HrCreatedAtOffset c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}