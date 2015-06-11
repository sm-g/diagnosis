using Diagnosis.Common;
using Diagnosis.Models;
using log4net;
using NHibernate;
using System.Collections.Generic;
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

        /// <summary>
        /// Будут сохранены только те, которых нет среди удаляемых.
        /// </summary>
        public bool DeleteAndSave(IEnumerable<IEntity> toDelete, IEnumerable<IEntity> toSave)
        {
            Contract.Requires(toDelete != null);
            Contract.Requires(toSave != null);
            Debug.Assert(session.IsOpen);

            toSave = toSave.Except(toDelete).ToList();
            logger.DebugFormat("deleting {0} IEntity", toDelete.Count());
            logger.DebugFormat("saving {0} IEntity", toSave.Count());

            using (var t = session.BeginTransaction())
            {
                try
                {
                    foreach (var item in toDelete)
                    {
                        session.Delete(item);
                    }
                    foreach (var item in toSave)
                    {
                        session.Save(item);
                    }
                    t.Commit();
                    logger.DebugFormat("deleted: {0}", string.Join<IEntity>("\n", toDelete));
                    logger.DebugFormat("saved: {0}", string.Join<IEntity>("\n", toSave));
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