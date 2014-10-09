using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Core
{
    public static class NameFormatter
    {
        /// <summary>
        /// короткое имя человека в форматах:
        /// Иванов И. И.
        /// Иванов И.
        /// Иванов
        /// Иван И.
        /// Иван
        /// </summary>
        /// <param name="man"></param>
        /// <returns></returns>
        public static string GetShortName(IMan man)
        {
            string ln = man.LastName ?? "";
            string mn = man.MiddleName ?? "";
            string fn = man.FirstName ?? "";

            var middle = mn.Length > 0 ? " " + mn[0] + "." : "";

            if (ln.Length > 0)
                return ln + (fn.Length > 0 ? " " + fn[0] + "." + middle : "");
            else if (fn.Length > 0)
                return fn + middle;
            return "";
        }
    }
}
