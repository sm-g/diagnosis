using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;

namespace Diagnosis.Data
{
    public static class SqlCeHelper
    {
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
