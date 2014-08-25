using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Diagnosis.ViewModels
{
    public static class HierarchicalExtensions
    {
        /// <summary>
        /// Применяет действие к элементу и всем его детям.
        /// </summary>
        public static void ForBranch<T>(this T element, Action<T> action) where T : class, IHierarchical<T>
        {
            action(element);
            foreach (var item in element.AllChildren)
            {
                action(item);
            }
        }
    }
}
