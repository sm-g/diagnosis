using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using log4net;
using NHibernate;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Data
{
    public static class Saver
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Saver));

        public static bool DoDelete(this ISession session, params IEntity[] entities)
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
                    OnException(session, t, e);

                    return false;
                }
            }
        }

        public static bool DoSave(this ISession session, params IEntity[] entities)
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
                    OnException(session, t, e);

                    return false;
                }
            }
        }

        /// <summary>
        /// Будут сохранены только те, которых нет среди удаляемых.
        /// </summary>
        public static bool DeleteAndSave(this ISession session, IEnumerable<IEntity> toDelete, IEnumerable<IEntity> toSave)
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
                    OnException(session, t, e);

                    return false;
                }
            }
        }

        private static void OnException(ISession session, ITransaction t, System.Exception e)
        {
            t.Rollback();
            session.Close();

            logger.Error(e);

            NHibernateHelper.ReopenSession(session.SessionFactory);
            typeof(Saver).Send(Event.ShowMessageOverlay, new object[] { e.Message, typeof(Saver) }.AsParams(MessageKeys.String, MessageKeys.Type));
        }
    }
}