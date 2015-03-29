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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Diagnosis.Data
{
    [System.Serializable]
    public class FixedDefaultFlushEventListener : DefaultFlushEventListener
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected override void PerformExecutions(IEventSource session)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("executing flush");
            }
            try
            {
                session.ConnectionManager.FlushBeginning();
                session.PersistenceContext.Flushing = true;
                session.ActionQueue.PrepareActions();
                session.ActionQueue.ExecuteActions();
            }
            catch (HibernateException exception)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error("Could not synchronize database state with session", exception);
                }
                throw;
            }
            finally
            {
                session.PersistenceContext.Flushing = false;
                session.ConnectionManager.FlushEnding();
            }
        }
    }

    public class NHibernateHelper
    {
        private static readonly System.Lazy<NHibernateHelper> lazyInstance = new System.Lazy<NHibernateHelper>(() => new NHibernateHelper() { useSavedCfg = true });
        private Configuration _cfg;
        private HbmMapping _mapping;
        private ISession _session;
        private ISessionFactory _sessionFactory;
        private ConnectionInfo connection;
        private bool useSavedCfg;

        protected NHibernateHelper()
        {
        }

        public static NHibernateHelper Default { get { return lazyInstance.Value; } }

        public bool InMemory { get; set; }

        public bool ShowSql { get; set; }

        public bool FromTest { get; set; }

        public Configuration Configuration
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

        public string ConnectionString
        {
            get { return Configuration.GetProperty(Environment.ConnectionString); }
        }

        private HbmMapping Mapping
        {
            get { return _mapping ?? (_mapping = CreateMapping()); }
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

            var fail = false;

            // create db for client side
            if (side == Side.Client)
            {
                fail = conn.ProviderName != Constants.SqlCeProvider;

                if (!fail)
                    try
                    {
                        SqlHelper.CreateSqlCeByConStr(conn.ConnectionString);
                    }
                    catch (System.Exception)
                    {
                        fail = true;
                    }
            }

            fail |= !conn.IsAvailable();

            if (fail)
            {
                InMemory = true;
                connection = null;
                return false;
            }
            connection = new ConnectionInfo(conn.ConnectionString, conn.ProviderName);
            return true;
        }

        public ISession GetSession()
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

        public ISession OpenSession()
        {
            var s = SessionFactory.OpenSession();
            s.FlushMode = FlushMode.Commit;

            if (InMemory)
                InMemoryHelper.FillData(Configuration, s);
            if (FromTest) // same session in VMBase
                _session = s;
            return s;
        }

        public IStatelessSession OpenStatelessSession()
        {
            var s = SessionFactory.OpenStatelessSession();
            if (InMemory)
                InMemoryHelper.FillData(Configuration, s);
            return s;
        }

        private HbmMapping CreateMapping()
        {
            var mapper = new ModelMapper();
            var mapType = typeof(WordMap);
            var assemblyContainingMapping = Assembly.GetAssembly(mapType);
            var types = assemblyContainingMapping.GetExportedTypes().Where(t => t.Namespace == mapType.Namespace);
            mapper.AddMappings(types);
            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }

        private Configuration CreateConfiguration()
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

            cfg.EventListeners.FlushEventListeners = new IFlushEventListener[] { new FixedDefaultFlushEventListener() };

            cfg.AddMapping(Mapping);
            return cfg;
        }

        private void ConfigureSqlServer(Configuration cfg, string constr, bool showSql)
        {
            cfg.SetProperty(Environment.Dialect, typeof(MsSql2012Dialect).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionDriver, typeof(SqlClientDriver).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionProvider, typeof(DriverConnectionProvider).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionString, constr)
                .SetProperty(Environment.ShowSql, showSql ? "true" : "false");
        }

        private void ConfigureSqlCe(Configuration cfg, string constr, bool showSql)
        {
            cfg.SetProperty(Environment.ReleaseConnections, "on_close")
               .SetProperty(Environment.Dialect, typeof(MsSqlCe40Dialect).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionDriver, typeof(SqlServerCeDriver).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionProvider, typeof(DriverConnectionProvider).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionString, constr)
               .SetProperty(Environment.ShowSql, showSql ? "true" : "false");
        }

        private Configuration LoadConfiguration()
        {
            if (InMemory || IsConfigurationFileValid == false || !useSavedCfg)
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
            if (InMemory || !useSavedCfg)
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

        private void ExportSchemaToFile()
        {
            string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            new SchemaExport(Configuration)
                .SetDelimiter(";")
                .SetOutputFile(string.Format(desktop + "\\sqlite create {0:yyyy-MM-dd-HHmm}.sql", System.DateTime.Now))
                .Execute(true, false, false);
        }
    }
}