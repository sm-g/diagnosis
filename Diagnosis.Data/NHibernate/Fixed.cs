using Diagnosis.Common;
using NHibernate;
using NHibernate.Dialect;
using NHibernate.Event;
using NHibernate.Event.Default;
using System;
using System.Data;
using System.Linq;

namespace Diagnosis.Data.NHibernate
{
    [Serializable]
    public class FixedDefaultFlushEventListener : DefaultFlushEventListener
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected override void PerformExecutions(IEventSource session)
        {
            //if (log.IsDebugEnabled)
            //{
            //    log.Debug("executing flush");
            //}
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

    public class UpdatedMsSqlCe40Dialect : MsSqlCe40Dialect
    {
        public UpdatedMsSqlCe40Dialect()
            : base()
        {
            RegisterColumnType(DbType.Double, string.Format("NUMERIC({0}, {1})", Types.Numeric.Precision, Types.Numeric.Scale));
        }
    }
}