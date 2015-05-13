using System;
using System.Linq;

namespace Diagnosis.Data.Mappings
{
    internal class MappingHelper
    {
        public static bool MappingForSqlite { get; set; }
        public static string SqlDateTimeNow
        {
            get
            {
                if (MappingForSqlite)
                    return "now";
                else
                    return "GETDATE()"; // sqlserver, ce
            }
        }

        internal static void Reset()
        {
            MappingForSqlite = false;
        }
    }
}