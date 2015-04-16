using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data.Mappings;
using Diagnosis.Data.NHibernate;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Event;
using NHibernate.Event.Default;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Diagnosis.Data
{


    public class NHibernateHelper
    {
        private static readonly System.Lazy<NHibernateHelper> lazyInstance = new System.Lazy<NHibernateHelper>(() => new NHibernateHelper() { useSavedCfg = true });
        private Configuration _cfg;
        private HbmMapping _mapping;
        private ISession _session;
        private ISessionFactory _sessionFactory;
        private ConnectionInfo connection;
        private bool useSavedCfg;

        private bool inmem;

        protected NHibernateHelper()
        {
        }

        public static NHibernateHelper Default { get { return lazyInstance.Value; } }
        public bool InMemory
        {
            get { return inmem; }
            set
            {
                inmem = value;
                if (value) connection = null;
            }
        }

        public bool ShowSql { get; set; }

        public bool FromTest { get; set; }

        public Configuration Configuration
        {
            get
            {
                if (_cfg == null)
                {
                    if (connection == null && !InMemory)
                        throw new System.InvalidOperationException("First initialize with right connection.");

                    _cfg = LoadConfiguration();
                    if (_cfg == null)
                    {
                        _cfg = CreateConfiguration(connection, Mapping, ShowSql);
                        SaveConfiguration(_cfg);
                    }
                }
                return _cfg;
            }
        }

        public string ConnectionString
        {
            get { return Configuration.GetProperty(Environment.ConnectionString); }
        }

        private HbmMapping Mapping
        {
            get
            {
                if (_mapping == null)
                {
                    var provider = connection == null
                        ? Constants.SqliteProvider // inmem
                        : connection.ProviderName;
                    _mapping = CreateMapping(provider);
                }
                return _mapping;
            }
        }

        private ISessionFactory SessionFactory
        {
            get { return _sessionFactory ?? (_sessionFactory = Configuration.BuildSessionFactory()); }
        }

        private bool IsConfigurationFileValid
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

        public static NHibernateHelper FromServerConnectionInfo(ConnectionInfo conn)
        {
            var instance = new NHibernateHelper();
            instance.useSavedCfg = false;
            instance.Init(conn, Side.Server);

            return instance;
        }

        public static HbmMapping CreateMapping(string provider)
        {
            if (provider == Constants.SqliteProvider)
                Helper.MappingForSqlite = true;

            var mapper = new ModelMapper();
            var mapType = typeof(WordMap);
            var assemblyContainingMapping = Assembly.GetAssembly(mapType);
            var types = assemblyContainingMapping.GetExportedTypes().Where(t => t.Namespace == mapType.Namespace);
            mapper.AddMappings(types);
            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }

        public static Configuration CreateConfiguration(ConnectionInfo connection, HbmMapping mapping, bool showsql)
        {
            var cfg = new Configuration();

            if (connection == null)
                ConfigureSqlLiteInMemory(cfg, showsql);
            else
                switch (connection.ProviderName)
                {
                    case Constants.SqlCeProvider:
                        ConfigureSqlCe(cfg, connection.ConnectionString, showsql);
                        break;

                    case Constants.SqlServerProvider:
                        ConfigureSqlServer(cfg, connection.ConnectionString, showsql);
                        break;

                    default:
                        throw new System.NotSupportedException("ProviderName");
                }

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

            cfg.EventListeners.FlushEventListeners = new IFlushEventListener[] { new FixedDefaultFlushEventListener() };

            cfg.AddMapping(mapping);
            // ExportSchemaToFile(cfg);
            return cfg;
        }

        public static void ConfigureSqlServer(Configuration cfg, string constr, bool showSql)
        {
            cfg.SetProperty(Environment.Dialect, typeof(MsSql2012Dialect).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionDriver, typeof(SqlClientDriver).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionProvider, typeof(DriverConnectionProvider).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionString, constr)
                .SetProperty(Environment.ShowSql, showSql ? "true" : "false");
        }

        public static void ConfigureSqlCe(Configuration cfg, string constr, bool showSql)
        {
            cfg.SetProperty(Environment.ReleaseConnections, "on_close")
               .SetProperty(Environment.Dialect, typeof(UpdatedMsSqlCe40Dialect).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionDriver, typeof(SqlServerCeDriver).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionProvider, typeof(DriverConnectionProvider).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionString, constr)
               .SetProperty(Environment.ShowSql, showSql ? "true" : "false");
        }

        public static void ConfigureSqlLiteInMemory(Configuration cfg, bool showSql)
        {
            cfg.SetProperty(Environment.ReleaseConnections, "on_close")
               .SetProperty(Environment.Dialect, typeof(SQLiteDialect).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionDriver, typeof(SQLite20Driver).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionString, "data source=:memory:;BinaryGuid=False")
               .SetProperty(Environment.ShowSql, showSql ? "true" : "false");
        }

        /// <summary>
        /// Initialize connection, set InMemory if any error occured.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="side"></param>
        /// <returns>Connection success</returns>
        public bool Init(ConnectionInfo conn, Side side)
        {
            if (conn == null)
            {
                InMemory = true;
                return false;
            }

            var fail = !conn.IsAvailable();

            if (fail)
            {
                InMemory = true;
                connection = null;
                return false;
            }
            if (inmem)
                return false;
            connection = new ConnectionInfo(conn.ConnectionString, conn.ProviderName);
            return true;
        }

        public ISession GetSession()
        {
            if (_session == null)
            {
                _session = OpenSession();
            }
            return _session;
        }

        public ISession OpenSession()
        {
            //ExportSchemaToFile(Configuration);
            var s = SessionFactory.OpenSession();
            s.FlushMode = FlushMode.Commit;

            if (inmem)
            {
                new SchemaExport(Configuration).Execute(false, true, false, s.Connection, null);
                InMemoryHelper.FillData(Configuration, s);
            }
            if (FromTest) // same session in VMBase
                _session = s;
            return s;
        }

        private Configuration LoadConfiguration()
        {
            if (inmem || !useSavedCfg || IsConfigurationFileValid == false)
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

        private void SaveConfiguration(Configuration cfg)
        {
            if (inmem || !useSavedCfg)
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

        private static void ExportSchemaToFile(Configuration cfg)
        {
            string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            new SchemaExport(cfg)
                .SetDelimiter(";")
                .SetOutputFile(string.Format(desktop + "\\sqlite create {0:yyyy-MM-dd-HHmm}.sql", System.DateTime.Now))
                .Execute(true, false, false);
        }
    }



}