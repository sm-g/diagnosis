using Diagnosis.Data;
using NHibernate;

namespace Diagnosis.ViewModels
{
    public class SessionVMBase : ViewModelBase
    {
        // private ISession session;
        private IStatelessSession statelessSession;

        static SessionVMBase()
        {
            if (IsInDesignMode)
            {
                NHibernateHelper.Default.InMemory = true;
                NHibernateHelper.Default.ShowSql = false;
                NHibernateHelper.Default.FromTest = false;
            }
        }

        public SessionVMBase()
        {
        }

        protected IStatelessSession StatelessSession
        {
            get
            {
                if (statelessSession == null)
                    statelessSession = NHibernateHelper.Default.OpenStatelessSession();
                return statelessSession;
            }
        }

        protected ISession Session
        {
            get
            {
                return NHibernateHelper.Default.GetSession();
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //if (Session != null)
                //    Session.Dispose();
                //if (statelessSession != null)
                //    statelessSession.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}