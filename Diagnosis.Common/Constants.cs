using System;
using System.Linq;

namespace Diagnosis.Common
{
    public static class Constants
    {
        public static string serverConStrName = "server";
        public static string clientConStrName = "client";
        public static string SerializedConfig = AppDataDir + "Configuration.serialized";
        public static string LayoutFileName = AppDataDir + "avalon-layout.config";
        public static string BackupDir = AppDataDir + "Backup\\";

        public static bool IsClient { get { return System.Reflection.Assembly.GetEntryAssembly().FullName.Contains("Client"); } }

        public static string AppDataDir
        {
            get
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                    "\\Diagnosis\\" + (IsClient ? "Client\\" : "Server\\");
                FileHelper.CreateDirectoryForPath(dir);
                return dir;
            }
        }

        public const string SqlCeProvider = "System.Data.SqlServerCE.4.0";
        public const string SqlServerProvider = "System.Data.SqlClient";
    }
}