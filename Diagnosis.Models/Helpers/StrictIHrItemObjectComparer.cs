using System;
using System.Collections.Generic;
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
}
