using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{
    public static class Constants
    {
        public static string serverConStrName = "server";
        public static string clientConStrName = "client";
        public static string SerializedConfig = "Configuration.serialized";

        public const string SqlCeProvider = "System.Data.SqlServerCE.4.0";
        public const string SqlServerProvider = "System.Data.SqlClient";
    }
}
