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
        private const string SerializedConfig = "Configuration.serialized";
        private static Configuration _cfg;
        private static HbmMapping _mapping;
        private static ISession _session;
        private static ISessionFactory _sessionFactory;

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

        private static HbmMapping CreateMapping()
        {
            var mapper = new ModelMapper();
            var assemblyContainingMapping = Assembly.GetAssembly(typeof(WordMap));
            var types = assemblyContainingMapping.GetExportedTypes().Where(t => t.Namespace == "Diagnosis.Data.Mappings");
            mapper.AddMappings(types);
            return mapper.CompileMappingForAllExplicitlyAddedEntities();
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

        private static Configuration CreateConfiguration()
        {
            var cfg = new Configuration();

            if (InMemory)
                InMemoryHelper.Configure(cfg, ShowSql);
            else
                ConfigureSqlCe(cfg, ShowSql);

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

        static void ConfigureSqlCe(Configuration cfg, bool showSql)
        {
            cfg.SetProperty(Environment.ReleaseConnections, "on_close")
               .SetProperty(Environment.Dialect, typeof(MsSqlCe40Dialect).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionDriver, typeof(SqlServerCeDriver).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionProvider, typeof(DriverConnectionProvider).AssemblyQualifiedName)
               .SetProperty(Environment.ConnectionString, "Data Source=diagnosis.sdf;Persist Security Info=False;")
               .SetProperty(Environment.ShowSql, showSql ? "true" : "false");
        }

        private static Configuration LoadConfiguration()
        {
            if (InMemory || IsConfigurationFileValid == false)
                return null;
            try
            {
                using (Stream stream = File.OpenRead(SerializedConfig))
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
                using (Stream stream = File.OpenWrite(SerializedConfig))
                {
                    var serializer = new BinaryFormatter();
                    serializer.Serialize(stream, cfg);
                }
            }
            catch
            {
            }
        }

        private static bool IsConfigurationFileValid
        {
            get
            {
                // cfg.xml changing not updates assInfo.LastWriteTime
                var ass = Assembly.GetCallingAssembly();
                if (ass.Location == null)
                    return false;

                var serializedConfigInfo = new FileInfo(SerializedConfig);

                var appConfigFile = System.AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                var configFileInfo = new FileInfo(appConfigFile); // may be other connection string
                var assInfo = new FileInfo(ass.Location);

                if (serializedConfigInfo.LastWriteTime < assInfo.LastWriteTime ||
                    serializedConfigInfo.LastWriteTime < configFileInfo.LastWriteTime)
                    return false;
                return true;
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