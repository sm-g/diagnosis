﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Core
{

    /// <summary>
    /// Returns -1 instead of 1 if y is IsNullOrWhiteSpace when x is Not.
    /// </summary>
    public class EmptyStringsAreLast : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (String.IsNullOrWhiteSpace(y) && !String.IsNullOrWhiteSpace(x))
            {
                return -1;
            }
            else if (!String.IsNullOrWhiteSpace(y) && String.IsNullOrWhiteSpace(x))
            {
                return 1;
            }
            else
            {
                return String.Compare(x, y);
            }
        }
    }

}