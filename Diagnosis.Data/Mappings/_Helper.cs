using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Data.Mappings
{
    class Helper
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
