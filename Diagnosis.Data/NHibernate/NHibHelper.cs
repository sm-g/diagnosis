using Diagnosis.Common;
using Diagnosis.Data.Mappings;
using Diagnosis.Data.NHibernate;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Event;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Diagnosis.Data
{
    public static class NHibernateHelper
    {
        private static Configuration _cfg;
        private static HbmMapping _mapping;
        private static ISession _session;
        private static ISessionFactory _sessionFactory;
        private static System.Configuration.ConnectionStringSettings connection;

        public static bool InMemory { get; set; }

        public static bool ShowSql { get; set; }

        public static bool FromTest { get; set; }

        public static Configuration Configuration
        {
            get
            {
                if (_cfg == null)
                {
                    _cfg = LoadConfiguration();
                    if (_cfg == null)
                    {
                        _cfg = CreateConfiguration();
                        SaveConfiguration(_cfg);
                    }
                }
                return _cfg;
            }
        }

        public static string ConnectionString
        {
            get { return Configuration.GetProperty(Environment.ConnectionString); }
        }

        private static HbmMapping Mapping
        {
            get
            {
                return _mapping ?? (_mapping = CreateMapping());
            }
        }

        private static ISessionFactory SessionFactory
        {
            get
            {
                return _sessionFactory ?? (_sessionFactory = Configuration.BuildSessionFactory());
            }
        }

        private static bool IsConfigurationFileValid
        {
            get
            {
                // cfg.xml changing not updates assInfo.LastWriteTime
                var ass = Assembly.GetEntryAssembly();
                if (ass.Location == null)
                    return false;

                var serializedConfigInfo = new FileInfo(Constants.SerializedConfig);

                var appConfigFile = System.AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                var configFileInfo = new FileInfo(appConfigFile); // may be other connection string
                var assInfo = new FileInfo(ass.Location);

                if (serializedConfigInfo.LastWriteTime < assInfo.LastWriteTime ||
                    serializedConfigInfo.LastWriteTime < configFileInfo.LastWriteTime)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Initialize connection, set InMemory if any error occured.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="side"></param>
        /// <returns>Connection success</returns>
        public static bool Init(System.Configuration.ConnectionStringSettings conn, Side side)
        {
            if (conn == null)
            {
                InMemory = true;
                return false;
            }

            var fail = false;
            var constr = conn.ConnectionString.Replace("%APPDATA%",
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData));

            // create db for client side
            if (side == Side.Client)
            {
                fail = conn.ProviderName != Constants.SqlCeProvider;

                if (!fail)
                    try
                    {
                        SqlHelper.CreateSqlCeByConStr(constr);
                    }
                    catch (System.Exception)
                    {
                        fail = true;
                    }

            }

            connection = new System.Configuration.ConnectionStringSettings(conn.Name, constr, conn.ProviderName);
            fail |= !connection.IsAvailable();

            if (fail)
            {
                InMemory = true;
                return false;
            }
            return true;
        }

        public static ISession GetSession()
        {
            if (_session == null)
            {
                //ExportSchemaToFile();
                _session = SessionFactory.OpenSession();
                _session.FlushMode = FlushMode.Commit;

                if (InMemory)
                    InMemoryHelper.FillData(Configuration, _session);
            }
            return _session;
        }

        public static ISession OpenSession()
        {
            var s = SessionFactory.OpenSession();
            s.FlushMode = FlushMode.Commit;

            if (InMemory)
                InMemoryHelper.FillData(Configuration, s);
            if (FromTest)
                _session = s;
            return s;
        }

        public static IStatelessSession OpenStatelessSession()
        {
            var s = SessionFactory.OpenStatelessSession();
            if (InMemory)
                InMemoryHelper.FillData(Configuration, s);
            return s;
        }

        private static HbmMapping CreateMapping()
        {
            var mapper = new ModelMapper();
            var assemblyContainingMapping = Assembly.GetAssembly(typeof(WordMap));
            var types = assemblyContainingMapping.GetExportedTypes().Where(t => t.Namespace == "Diagnosis.Data.Mappings");
            mapper.AddMappings(types);
            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }

        private static Configuration CreateConfiguration()
        {
            var cfg = new Configuration();

            if (InMemory)
                InMemoryHelper.Configure(cfg, ShowSql);
            else if (connection != null)
                switch (connection.ProviderName)
                {
                    case Constants.SqlCeProvider:
                        ConfigureSqlCe(cfg, connection.ConnectionString, ShowSql);
                        break;

                    case Constants.SqlServerProvider:
                        ConfigureSqlServer(cfg, connection.ConnectionString, ShowSql);
                        break;

                    default:
                        throw new System.NotSupportedException("ProviderName");
                }
            else
                throw new System.InvalidOperationException("First initialize with right connection.");

            var preListener = new PreEventListener();
            cfg.AppendListeners(ListenerType.PreUpdate, new IPreUpdateEventListener[] { preListener });
            cfg.AppendListeners(ListenerType.PreInsert, new IPreInsertEventListener[] { preListener });
            cfg.AppendListeners(ListenerType.PreDelete, new IPreDeleteEventListener[] { preListener });
            cfg.AppendListeners(ListenerType.PreLoad, new IPreLoadEventListener[] { preListener });

            var postListener = new PostEventListener();
            cfg.AppendListeners(ListenerType.PostUpdate, new IPostUpdateEventListener[] { postListener });
            cfg.AppendListeners(ListenerType.PostInsert, new IPostInsertEventListener[] { postListener });
            cfg.AppendListeners(ListenerType.PostDelete, new IPostDeleteEventListener[] { postListener });
            cfg.AppendListeners(ListenerType.PostLoad, new IPostLoadEventListener[] { postListener });

            cfg.AddMapping(Mapping);
            return cfg;
        }

        private static void ConfigureSqlServer(Configuration cfg, string constr, bool showSql)
        {
            cfg.SetProperty(Environment.Dialect, typeof(MsSql2012Dialect).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionDriver, typeof(SqlClientDriver).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionProvider, typeof(DriverConnectionProvider).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionString, constr)
                .SetProperty(Environment.ShowSql, showSql ? "true" : "false");
        }

        private static void ConfigureSqlCe(Configuration cfg, string constr, bool showSql)
        {
            cfg.SetProperty(Environment.ReleaseConnections, "on_close")
               .SetProperty(Environment.Dialect, typeof(MsSqlCe40Dialect).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionDriver, typeof(SqlServerCeDriver).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionProvider, typeof(DriverConnectionProvider).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionString, constr)
               .SetProperty(Environment.ShowSql, showSql ? "true" : "false");
        }

        private static Configuration LoadConfiguration()
        {
            if (InMemory || IsConfigurationFileValid == false)
                return null;
            try
            {
                using (Stream stream = File.OpenRead(Constants.SerializedConfig))
                {
                    var serializer = new BinaryFormatter();
                    return serializer.Deserialize(stream) as Configuration;
                }
            }
            catch
            {
                return null;
            }
        }

        private static void SaveConfiguration(Configuration cfg)
        {
            if (InMemory)
                return;

            try
            {
                using (Stream stream = File.OpenWrite(Constants.SerializedConfig))
                {
                    var serializer = new BinaryFormatter();
                    serializer.Serialize(stream, cfg);
                }
            }
            catch
            {
            }
        }

        private static void ExportSchemaToFile()
        {
            string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            new SchemaExport(Configuration)
                .SetDelimiter(";")
                .SetOutputFile(string.Format(desktop + "\\sqlite create {0:yyyy-MM-dd-HHmm}.sql", System.DateTime.Now))
                .Execute(true, false, false);
        }
    }
}