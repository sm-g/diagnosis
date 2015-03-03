using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Diagnosis.Common
{
    public static class Constants
    {
        static bool isClient = Assembly.GetEntryAssembly().FullName.Contains("Client");
        static string _appDataDir;

        public static string serverConStrName = "server";
        public static string clientConStrName = "client";
        public static string clientConfigFileName = "Diagnosis.Client.App.exe.config";
        public static string serverConfigFileName = "Diagnosis.Server.App.exe.config";

        public static string SerializedConfig = AppDataDir + "Configuration.serialized";
        public static string LayoutFileName = AppDataDir + "avalon-layout.config";
        public static string BackupDir = AppDataDir + "Backup\\";
        public static string HelpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Help\\");

        public static bool IsClient { get { return isClient; } }

        public static string AppDataDir
        {
            get
            {
                if (_appDataDir == null)
                {
                    _appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                        "\\Diagnosis\\" + (IsClient ? "Client\\" : "Server\\");
                    FileHelper.CreateDirectoryForPath(_appDataDir);
                }
                return _appDataDir;
            }
        }

        public const string SqlCeProvider = "System.Data.SqlServerCE.4.0";
        public const string SqlServerProvider = "System.Data.SqlClient";

        public static string ClientConfigFilePath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, clientConfigFileName); } }
        public static string ServerConfigFilePath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, serverConfigFileName); } }
    }
}