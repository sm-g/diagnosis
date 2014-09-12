using Diagnosis.Models;
using NHibernate.Event;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Diagnosis.Data
{
    internal interface IAuditLogger
    {
        void Delete(EntityBase entity);

        void Insert(EntityBase entity);

        void Update(EntityBase entity);

        void Load(EntityBase entityBase);
    }

    [Serializable]
    internal class AuditLogger : IAuditLogger
    {
        public void Insert(EntityBase entity)
        {
            Debug.Print("{0} #{1} inserted.", entity.GetType(), entity.Id);
        }

        public void Update(EntityBase entity)
        {
            Debug.Print("{0} #{1} updated.", entity.GetType(), entity.Id);
        }

        public void Delete(EntityBase entity)
        {
            Debug.Print("{0} #{1} deleted.", entity.GetType(), entity.Id);
        }

        public void Load(EntityBase entity)
        {
            Debug.Print("{0} #{1} loaded.", entity.GetType(), entity.Id);
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
            _logger.Insert(e.Entity as EntityBase);
            return false;
        }

        public bool OnPreUpdate(PreUpdateEvent e)
        {
            _logger.Update(e.Entity as EntityBase);
            return false;
        }

        public bool OnPreDelete(PreDeleteEvent e)
        {
            _logger.Delete(e.Entity as EntityBase);
            return false;
        }

        public void OnPreLoad(PreLoadEvent e)
        {
            _logger.Load(e.Entity as EntityBase);
        }
    }


}