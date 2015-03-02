using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;

namespace Diagnosis.Data
{
    public static class SqlHelper
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

        public static bool IsAvailable(this ConnectionStringSettings connection)
        {
            try
            {
                var dbConn = CreateConnection(connection.ConnectionString, connection.ProviderName);
                return dbConn.IsAvailable();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static DbConnection CreateConnection(string connstr, string provider)
        {
            switch (provider)
            {
                case Constants.SqlCeProvider:
                    return new SqlCeConnection(connstr);

                case Constants.SqlServerProvider:
                    return new SqlConnection(connstr);

                default:
                    throw new NotSupportedException();
            }

        }

        public static void CreateSqlCeByConStr(string constr)
        {
            var builder = new SqlCeConnectionStringBuilder(constr);
            var sdfPath = builder.DataSource;
            if (!System.IO.File.Exists(sdfPath))
            {
                FileHelper.CreateDirectoryForPath(sdfPath);

                using (var engine = new SqlCeEngine(constr))
                {
                    engine.CreateDatabase();
                }
            }
        }

        public static void CreateSqlCeByPath(string sdfPath)
        {
            if (!System.IO.File.Exists(sdfPath))
            {
                FileHelper.CreateDirectoryForPath(sdfPath);

                using (var engine = new SqlCeEngine("Data Source=" + sdfPath))
                {
                    engine.CreateDatabase();
                }
            }
        }
    }
}
