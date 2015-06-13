using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public static class NavigatorHelper
    {
        public static T FindHolderKeeperOf<T>(this IEnumerable<T> root, IHrsHolder holder)
            where T : HierarchicalBase<T>, IHolderKeeper
        {
            holder = holder.Actual as IHrsHolder;
            T vm;
            foreach (var item in root)
            {
                if (item.Holder == holder)
                    return item;
                vm = item.AllChildren.Where(x => x.Holder == holder).FirstOrDefault();
                if (vm != null)
                    return vm;
            }
            return null;
        }

        public static T FindCritKeeperOf<T>(this IEnumerable<T> root, ICrit crit)
           where T : HierarchicalBase<T>, ICritKeeper
        {
            crit = crit.Actual as ICrit;
            T vm;
            foreach (var item in root)
            {
                if (item.Crit == crit)
                    return item;
                vm = item.AllChildren.Where(x => x.Crit == crit).FirstOrDefault();
                if (vm != null)
                    return vm;
            }
            return null;
        }
    }
}