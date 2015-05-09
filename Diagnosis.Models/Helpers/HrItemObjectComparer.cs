using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class StrictIHrItemObjectComparer : IComparer<IHrItemObject>
    {
        public static int StrictCompare(IHrItemObject x, IHrItemObject y)
        {
            if (object.ReferenceEquals(x, null))
                return -1;

            if (object.ReferenceEquals(y, null))
                return 1;

            if (x is Measure)
                return (x as Measure).StrictCompareTo(y);

            return x.CompareTo(y);
        }

        public int Compare(IHrItemObject x, IHrItemObject y)
        {
            return StrictCompare(x, y);
        }
    }

    public class HrItemObjectComparer : IComparer<IHrItemObject>
    {
        // Icd < Measure < Comment < Word
        private static Dictionary<Type, int> priorities = new Dictionary<Type, int>();
        static HrItemObjectComparer()
        {
            priorities.Add(typeof(IcdDisease), 1);
            priorities.Add(typeof(Measure), 2);
            priorities.Add(typeof(Comment), 3);
            priorities.Add(typeof(Word), 4);
        }
        private int PriorityFor(Type type)
        {
            var actualType = priorities.Keys.FirstOrDefault(x => x.IsAssignableFrom(type));
            if (actualType == null)
                return int.MinValue;

            return priorities[actualType];
        }

        public int Compare(IHrItemObject x, IHrItemObject y)
        {
            // always use IHrItemObject.CompareTo to compare same types
            Contract.Ensures(Contract.Result<int>() != 0);

            int p1 = PriorityFor(x.GetType());
            int p2 = PriorityFor(y.GetType());
            return p1.CompareTo(p2);
        }
    }

    public class HrsHolderComparer : IComparer<IHrsHolder>
    {
        // App < Course < Patient
        private static Dictionary<Type, int> priorities = new Dictionary<Type, int>();
        static HrsHolderComparer()
        {
            priorities.Add(typeof(Appointment), 1);
            priorities.Add(typeof(Course), 2);
            priorities.Add(typeof(Patient), 3);
        }
        private int PriorityFor(Type type)
        {
            var actualType = priorities.Keys.FirstOrDefault(x => x.IsAssignableFrom(type));
            if (actualType == null)
                return int.MinValue;

            return priorities[actualType];
        }

        public int Compare(IHrsHolder x, IHrsHolder y)
        {
            // always use IHrsHolder.CompareTo to compare same types
            Contract.Ensures(Contract.Result<int>() != 0);

            int p1 = PriorityFor(x.GetType());
            int p2 = PriorityFor(y.GetType());
            return p1.CompareTo(p2);
        }
    }
}
