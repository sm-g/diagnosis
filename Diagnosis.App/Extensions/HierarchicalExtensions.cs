using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    public static class HierarchicalExtensions
    {
        public static void ForAll<T>(this IList<T> list, Action<T> action) where T : class, IHierarchical<T>
        {
            Contract.Assume(list.Count > 0);

            foreach (var item in list[0].Parent.AllChildren)
            {
                action(item);
            }
        }
    }
}
