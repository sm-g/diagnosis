using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{
    public class Plurals
    {
        /// <summary>
        /// 0 - день, 1 - дня, 2 - дней
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int GetPluralEnding(int count)
        {
            int ending;
            if (count % 10 == 0 || count % 10 >= 5 || (count >= 11 && count <= 14))
            {
                ending = 2;
            }
            else if (count % 10 == 1)
            {
                ending = 0;
            }
            else
            {
                ending = 1;
            }
            return ending;
        }
    }
}
