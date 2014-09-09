using Diagnosis.Data.Mappings;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using NHibernate.Event;

namespace Diagnosis.Data
{
    public class NHibernateHelper
    {
        private const string filenameTemplate = "Configuration.serialized";
        private static Configuration _configuration;
        private static HbmMapping _mapping;
        private static ISession _session;
        private static ISessionFactory _sessionFactory;

        public static Configuration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = CreateConfiguration();
                }
                return _configuration;
            }
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

        public static HbmMapping CreateMapping()
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
                _session = SessionFactory.OpenSession();
            }
            return _session;
        }
        public static ISession OpenSession()
        {
            var s = SessionFactory.OpenSession();
            s.FlushMode = FlushMode.Auto;
            return s;
        }
        public static IStatelessSession OpenStatelessSession()
        {
            var s = SessionFactory.OpenStatelessSession();
            return s;
        }
        private static Configuration CreateConfiguration()
        {
            var cfg = new Configuration();
            cfg.Configure("NHibernate\\nhibernate.cfg.xml");
            cfg.Properties[Environment.CollectionTypeFactoryClass] = typeof(Net4CollectionTypeFactory).AssemblyQualifiedName;
            var listener = new EventListener();
            cfg.AppendListeners(ListenerType.PreUpdate, new IPreUpdateEventListener[] { listener });
            cfg.AppendListeners(ListenerType.PreInsert, new IPreInsertEventListener[] { listener });
            cfg.AppendListeners(ListenerType.PreDelete, new IPreDeleteEventListener[] { listener });
            cfg.AppendListeners(ListenerType.PreLoad, new IPreLoadEventListener[] { listener });

            cfg.AddMapping(Mapping);

            return cfg;
        }

        private static Configuration LoadConfiguration()
        {
            Configuration cfg;
            var serializer = new BinaryFormatter();
            using (Stream stream = File.OpenRead(filenameTemplate))
            {
                cfg = serializer.Deserialize(stream) as Configuration;
            }
            return cfg;
        }

        private static void SaveConfiguration(Configuration cfg)
        {
            var serializer = new BinaryFormatter();
            using (Stream stream = File.OpenWrite(filenameTemplate))
            {
                serializer.Serialize(stream, cfg);
            }
        }
    }
}