using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data.Mappings;
using Diagnosis.Data.Mappings.Client;
using Diagnosis.Data.Mappings.Server;
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
    public class NHibernateHelper
    {
        private static readonly System.Lazy<NHibernateHelper> lazyInstance = new System.Lazy<NHibernateHelper>(() => new NHibernateHelper());
        private Configuration _cfg;
        private HbmMapping _mapping;
        private ISession _session;
        private ISessionFactory _sessionFactory;
        private ConnectionInfo connection;
        private bool useSavedCfg;

        private bool inmem;
        private const string sqliteInmemoryConstr = "Data Source=:memory:;Version=3;New=True;BinaryGuid=False";
        private Side side;

        protected NHibernateHelper()
        {
            useSavedCfg = true;
        }

        /// <summary>
        /// Хелпер приложения, использует сохраненный конфиг. Требует инициализации.
        /// </summary>
        public static NHibernateHelper Default { get { return lazyInstance.Value; } }

        public bool InMemory
        {
            get { return inmem; }
            set
            {
                inmem = value;
                if (value)
                {
                    connection = new ConnectionInfo(sqliteInmemoryConstr, Constants.SqliteProvider);
                    useSavedCfg = false;
                }
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
                    if (connection == default(ConnectionInfo) && !InMemory)
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
                    _mapping = CreateMapping(connection.ProviderName, side);
                }
                return _mapping;
            }
        }

        private ISessionFactory SessionFactory
        {
            get { return _sessionFactory ?? (_sessionFactory = Configuration.BuildSessionFactory()); }
        }

        /// <summary>
        /// Хелпер для указанного подключения. Не требует инициализации.
        /// </summary>
        public static NHibernateHelper FromConnectionInfo(ConnectionInfo conn, Side side)
        {
            var instance = new NHibernateHelper();
            instance.Init(conn, side, false);

            return instance;
        }

        public static HbmMapping CreateMapping(string provider, Side side)
        {
            if (provider == Constants.SqliteProvider)
                MappingHelper.MappingForSqlite = true;

            var mapper = new ModelMapper();

            var sideMapType = side == Side.Client ? typeof(WordMapClient) : typeof(WordMapServer);
            var commonMapType = typeof(MappingHelper);
            var assemblyContainingMapping = Assembly.GetAssembly(sideMapType);
            var types = assemblyContainingMapping.GetExportedTypes().Where(t => t.Namespace == sideMapType.Namespace || t.Namespace == commonMapType.Namespace);
            mapper.AddMappings(types);
            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }

        public static Configuration CreateConfiguration(ConnectionInfo connection, HbmMapping mapping, bool showsql)
        {
            var cfg = new Configuration();

            switch (connection.ProviderName)
            {
                case Constants.SqlCeProvider:
                    ConfigureSqlCe(cfg, connection.ConnectionString, showsql);
                    break;

                case Constants.SqlServerProvider:
                    ConfigureSqlServer(cfg, connection.ConnectionString, showsql);
                    break;

                case Constants.SqliteProvider:
                    ConfigureSqlLiteInMemory(cfg, connection.ConnectionString, showsql);
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

        public static void ConfigureSqlLiteInMemory(Configuration cfg, string constr, bool showSql)
        {
            cfg.SetProperty(Environment.ReleaseConnections, "on_close")
               .SetProperty(Environment.Dialect, typeof(SQLiteDialect).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionDriver, typeof(SQLite20Driver).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionString, constr)
               .SetProperty(Environment.ShowSql, showSql ? "true" : "false");
        }

        /// <summary>
        /// Initialize connection, set InMemory if any error occured.
        /// </summary>
        /// <param name="conn"></param>
        ///
        /// <returns>Connection success</returns>
        public bool Init(ConnectionInfo conn, Side side, bool useSavedCfg = true)
        {
            if (connection != default(ConnectionInfo))
                throw new System.InvalidOperationException("Already initialized");

            this.useSavedCfg = useSavedCfg;
            this.side = side;

            var fail = !conn.IsAvailable();

            if (fail || conn == default(ConnectionInfo))
                InMemory = true;

            if (InMemory) // maybe set before Init call
                return false;

            connection = conn;
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
            //ExportSchemaToFile(Configuration, side);
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
            if (!useSavedCfg || !IsConfigurationFileValid())
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
            if (!useSavedCfg)
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

        private static bool IsConfigurationFileValid()
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

        private static void ExportSchemaToFile(Configuration cfg, Side side)
        {
            string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            new SchemaExport(cfg)
                .SetDelimiter(";")
                .SetOutputFile(string.Format(desktop + "\\sqlite create {0} {1:yyyy-MM-dd-HHmmss}.sql", side, System.DateTime.Now))
                .Execute(true, false, false);
        }
    }
}