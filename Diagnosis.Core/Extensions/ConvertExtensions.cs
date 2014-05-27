using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.App.ViewModels
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
    }
}
