﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{
    public static class ConvertExtensions
    {
        public static TDest? ConvertTo<TSource, TDest>(this TSource? source)
            where TDest : struct
            where TSource : struct
        {
            if (source == null)
            {
                return null;
            }
            return (TDest)Convert.ChangeType(source.Value, typeof(TDest));
        }

        public static T As<T>(this object obj)
        {
            if (obj == null)
                return default(T);
            return (T)obj;
        }
    }
}
