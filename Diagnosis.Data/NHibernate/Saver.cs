using Diagnosis.Common;
using Diagnosis.Models;
using log4net;
using NHibernate;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Data
{
    public class Saver
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Saver));

        private Patient savingPatient;
        private ISession session;

        public Saver(ISession session)
        {
            this.session = session;
        }


        public bool Delete(params IEntity[] entities)
        {
            Contract.Requires(entities != null);

            if (entities.Length == 0) return true;
            logger.DebugFormat("deleting {0} IEntity", entities.Length);
            Debug.Assert(session.IsOpen);

            using (var t = session.BeginTransaction())
            {
                try
                {
                    foreach (var item in entities)
                    {
                        session.Delete(item);
                    }
                    t.Commit();
                    logger.DebugFormat("deleted: {0}", string.Join<IEntity>("\n", entities));
                    return true;
                }
                catch (System.Exception e)
                {
                    t.Rollback();
                    logger.Error(e);
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
            }
        }

        public bool Save(params IEntity[] entities)
        {
            Contract.Requires(entities != null);

            if (entities.Length == 0) return true;
            logger.DebugFormat("saving {0} IEntity", entities.Length);
            Debug.Assert(session.IsOpen);

            using (var t = session.BeginTransaction())
            {
                try
                {
                    foreach (var item in entities)
                    {
                        session.SaveOrUpdate(item);
                    }
                    t.Commit();
                    logger.DebugFormat("saved: {0}", string.Join<IEntity>("\n", entities));
                    return true;
                }
                catch (System.Exception e)
                {
                    t.Rollback();
                    logger.Error(e);
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
            }
        }
    }
}