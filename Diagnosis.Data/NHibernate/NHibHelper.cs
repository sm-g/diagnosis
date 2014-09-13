#if DEBUG
#define MEMORY
//#define LOG
#endif

using Diagnosis.Data.Mappings;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Event;
using NHibernate.Mapping.ByCode;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Diagnosis.Data
{
    public class NHibernateHelper
    {
        private const string SerializedConfig = "NHibernate\\Configuration.serialized";
        private const string ConfigFile = "NHibernate\\nhibernate.cfg.xml";
        private static Configuration _cfg;
        private static HbmMapping _mapping;
        private static ISession _session;
        private static ISessionFactory _sessionFactory;

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
#if MEMORY
                InMemoryHelper.FillData(Configuration, _session);
#endif
            }
            return _session;
        }

        public static ISession OpenSession()
        {
            var s = SessionFactory.OpenSession();
            s.FlushMode = FlushMode.Auto;
#if MEMORY
            InMemoryHelper.FillData(Configuration, s);
#endif
            return s;
        }

        public static IStatelessSession OpenStatelessSession()
        {
            var s = SessionFactory.OpenStatelessSession();
#if MEMORY
            InMemoryHelper.FillData(Configuration, s);
#endif
            return s;
        }

        private static Configuration CreateConfiguration()
        {
            var cfg = new Configuration();
#if !MEMORY
            cfg.Configure(ConfigFile);
#else
            InMemoryHelper.Configure(cfg);
#endif
#if LOG
            var preListener = new PreEventListener();
            cfg.AppendListeners(ListenerType.PreUpdate, new IPreUpdateEventListener[] { preListener });
            cfg.AppendListeners(ListenerType.PreInsert, new IPreInsertEventListener[] { preListener });
            cfg.AppendListeners(ListenerType.PreDelete, new IPreDeleteEventListener[] { preListener });
            cfg.AppendListeners(ListenerType.PreLoad, new IPreLoadEventListener[] { preListener });
#endif
            var postListener = new PostEventListener();
            cfg.AppendListeners(ListenerType.PostUpdate, new IPostUpdateEventListener[] { postListener });
            cfg.AppendListeners(ListenerType.PostInsert, new IPostInsertEventListener[] { postListener });
            cfg.AppendListeners(ListenerType.PostDelete, new IPostDeleteEventListener[] { postListener });
            cfg.AppendListeners(ListenerType.PostLoad, new IPostLoadEventListener[] { postListener });

            cfg.AddMapping(Mapping);
            return cfg;
        }

        private static Configuration LoadConfiguration()
        {
#if MEMORY
            return null;
#endif
            if (IsConfigurationFileValid == false)
                return null;
            try
            {
                using (Stream stream = File.OpenRead(SerializedConfig))
                {
                    var serializer = new BinaryFormatter();
                    return serializer.Deserialize(stream) as Configuration;
                }
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private static void SaveConfiguration(Configuration cfg)
        {
            using (Stream stream = File.OpenWrite(SerializedConfig))
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(stream, cfg);
            }
        }

        private static bool IsConfigurationFileValid
        {
            get
            {
                var ass = Assembly.GetCallingAssembly();
                if (ass.Location == null)
                    return false;
                var configInfo = new FileInfo(SerializedConfig);
                var assInfo = new FileInfo(ass.Location);
                var configFileInfo = new FileInfo(ConfigFile);
                if (configInfo.LastWriteTime < assInfo.LastWriteTime)
                    return false;
                if (configInfo.LastWriteTime < configFileInfo.LastWriteTime)
                    return false;
                return true;
            }
        }
    }
}