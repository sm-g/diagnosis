using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Diagnosis.Data.Sync
{
    public static class SqlExtensions
    {
        public static bool IsAvailable(this DbConnection connection)
        {
            try
            {
                connection.Open();
                connection.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
