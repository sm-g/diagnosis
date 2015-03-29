using Diagnosis.Common.Types;
using Diagnosis.Common.Util;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Diagnosis.Common
{
    public static class Constants
    {
        // TODO fix for tests
        private static AssemblyInfo info = new AssemblyInfo(Assembly.GetEntryAssembly());
        private static bool isClient = Assembly.GetEntryAssembly().FullName.Contains("Client") || Assembly.GetEntryAssembly().FullName.Contains("Test");
        private static string _localAppDataDir;

        public const string SqlCeProvider = "System.Data.SqlServerCE.4.0";
        public const string SqlServerProvider = "System.Data.SqlClient";

        public static string serverConStrName = "server";
        public static string clientConStrName = "client";

        public static string clientConfigFileName = "Diagnosis.Client.App.exe.config";
        public static string serverConfigFileName = "Diagnosis.Server.App.exe.config";

        public static string productName = info.Product;
        public static string companyName = info.Company;

        public static string SerializedConfig = AppDataDir + "Configuration.serialized";
        public static string LayoutFileName = AppDataDir + "avalon-layout.config";
        public static string BackupDir = AppDataDir + "Backup\\";
        public static string HelpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Help\\");

        public static bool IsClient { get { return isClient; } }

        public static string AppDataDir
        {
            get
            {
                if (_localAppDataDir == null)
                {
                    _localAppDataDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        companyName.Replace(' ', '_'),
                        productName,
                        (IsClient ? "Client\\" : "Server\\"));
                    FileHelper.CreateDirectoryForPath(_localAppDataDir);
                }
                return _localAppDataDir;
            }
        }

        public static string ClientConfigFilePath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, clientConfigFileName); } }

        public static string ServerConfigFilePath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, serverConfigFileName); } }

        /// <summary>
        /// Для доступа к Settings из ViewModel
        /// </summary>
        public static ConnectionInfo ServerConnectionInfo { get; set; }

        public static string SyncServerConstrSettingName { get; set; }

        public static string SyncServerProviderSettingName { get; set; }

        public static string ExpandVariables(this string str)
        {
            return str
                .Replace("%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                .Replace("%LOCALAPPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }
    }
}