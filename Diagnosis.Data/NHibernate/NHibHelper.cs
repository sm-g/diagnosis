using Diagnosis.Models;
using Diagnosis.Data.Mappings;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using System.Collections.Generic;
using System.Reflection;

namespace Diagnosis.Data
{
    public class NHibernateHelper
    {
        private static Configuration _configuration;
        private static HbmMapping _mapping;
        private static ISessionFactory _sessionFactory;
        static ISession _session;

        public static Configuration Configuration
        {
            get
            {
                return _configuration ?? (_configuration = CreateConfiguration());
            }
        }

        public static HbmMapping Mapping
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

        public static ISession GetSession()
        {
            return _session ?? (_session = SessionFactory.OpenSession());
        }

        private static Configuration CreateConfiguration()
        {
            var cfg = new Configuration();

            cfg.Configure("NHibernate\\hibernate.cfg.xml");
            cfg.Properties[Environment.CollectionTypeFactoryClass] = typeof(Net4CollectionTypeFactory).AssemblyQualifiedName;
            cfg.AddAssembly(typeof(Patient).Assembly);
            cfg.AddMapping(Mapping);

            return cfg;
        }

        private static HbmMapping CreateMapping()
        {

            var mapper = new ModelMapper();
            var types = Assembly.GetExecutingAssembly().GetExportedTypes();

            mapper.AddMappings(types);

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }
    }
}