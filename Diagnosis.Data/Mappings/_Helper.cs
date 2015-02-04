using System;
using System.Linq;

namespace Diagnosis.Data.Mappings
{
    internal class Helper
    {
        public static string SqlDateTimeNow
        {
            get
            {
                if (NHibernateHelper.InMemory)
                    return "now"; // sqlite
                else
                    return "GETDATE()"; // sqlserver ce
            }
        }
    }
}