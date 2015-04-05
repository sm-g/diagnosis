using Diagnosis.Common.Types;
using Diagnosis.Common.Util;
using HdSystemLibrary.Reflection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Diagnosis.Common
{
    public static class Constants
    {
        private static bool? _isClient;
        private static string _localAppDataDir;

        public const string SqlCeProvider = "System.Data.SqlServerCE.4.0";
        public const string SqlServerProvider = "System.Data.SqlClient";

        public static string serverConStrName = "server";
        public static string clientConStrName = "client";

        public static string clientConfigFileName = "Diagnosis.Client.App.exe.config";
        public static string serverConfigFileName = "Diagnosis.Server.App.exe.config";

        public static string SerializedConfig { get { return AppDataDir + "Configuration.serialized"; } }
        public static string LayoutFileName { get { return AppDataDir + "avalon-layout.config"; } }
        public static string BackupDir { get { return AppDataDir + "Backup\\"; } }

        public static string HelpDir { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Help\\"); } }

        public static bool IsClient
        {
            get
            {
                if (_isClient == null)
                {
                    _isClient = Assembly.GetEntryAssembly().FullName.Contains("Client");
                }
                return _isClient.Value;
            }
            set // for tests
            {
                _isClient = value;
            }
        }

        public static string AppDataDir
        {
            get
            {
                if (_localAppDataDir == null)
                {
                    var info = new AssemblyInfo(Assembly.GetEntryAssembly());
                    _localAppDataDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        info.Company.Replace(' ', '_'),
                        info.Product,
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