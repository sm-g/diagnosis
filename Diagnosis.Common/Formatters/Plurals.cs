using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{
    public class PluralsHelper
    {
        public static string[] minutes = new string[3] { "минута", "минуты", "минут" };
        public static string[] hours = new string[3] { "час", "часа", "часов" };
        public static string[] days = new string[3] { "день", "дня", "дней" };
        public static string[] weeks = new string[3] { "неделя", "недели", "недель" };
        public static string[] months = new string[3] { "месяц", "месяца", "месяцев" };
        public static string[] years = new string[3] { "год", "года", "лет" };

        /// <summary>
        /// 0 - день, 1 - дня, 2 - дней
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int GetPluralEnding(int count)
        {
            int ending;
            count = Math.Abs(count);
            if (count % 10 == 0 || count % 10 >= 5 || (count >= 11 && count <= 14))
                ending = 2;
            else if (count % 10 == 1)
                ending = 0;
            else
                ending = 1;

            return ending;
        }
    }
}
