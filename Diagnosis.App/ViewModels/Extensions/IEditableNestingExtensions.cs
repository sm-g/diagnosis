using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    static class IEditableNestingExtensions
    {
        public static void DeleteEmpty<T>(this IEditableNesting outer, IList<T> inners) where T : IEditableNesting
        {
            var i = 0;
            while (i < inners.Count)
            {
                if (inners[i].IsEmpty)
                {
                    inners[i].Editable.DeleteCommand.Execute(null);
                }
                else
                {
                    i++;
                }
            }
        }
    }
}
