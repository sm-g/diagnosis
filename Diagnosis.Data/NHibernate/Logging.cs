using Diagnosis.Models;
using NHibernate.Event;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;

namespace Diagnosis.Data
{
    internal interface IAuditLogger
    {
        void Delete(IEntity entity);

        void Insert(IEntity entity);

        void Update(IEntity entity);

        void Load(IEntity entity);
    }

    [Serializable]
    internal class AuditLogger : IAuditLogger
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AuditLogger));

        public void Insert(IEntity entity)
        {
            logger.InfoFormat("{0} #{1} inserted.", entity.GetType(), entity.Id);
        }

        public void Update(IEntity entity)
        {
            logger.InfoFormat("{0} #{1} updated.", entity.GetType(), entity.Id);
        }

        public void Delete(IEntity entity)
        {
            logger.InfoFormat("{0} #{1} deleted.", entity.GetType(), entity.Id);
        }

        public void Load(IEntity entity)
        {
            logger.InfoFormat("{0} #{1} loaded.", entity.GetType(), entity.Id);
        }
    }

    [Serializable]
    internal class PreEventListener : IPreInsertEventListener, IPreUpdateEventListener, IPreDeleteEventListener, IPreLoadEventListener
    {
        private readonly IAuditLogger _logger;

        public PreEventListener()
            : this(new AuditLogger())
        { }

        public PreEventListener(IAuditLogger logger)
        {
            _logger = logger;
        }

        public bool OnPreInsert(PreInsertEvent e)
        {
            _logger.Insert(e.Entity as IEntity);
            return false;
        }

        public bool OnPreUpdate(PreUpdateEvent e)
        {
            _logger.Update(e.Entity as IEntity);
            return false;
        }

        public bool OnPreDelete(PreDeleteEvent e)
        {
            _logger.Delete(e.Entity as IEntity);
            return false;
        }

        public void OnPreLoad(PreLoadEvent e)
        {
            _logger.Load(e.Entity as IEntity);
        }
    }


}