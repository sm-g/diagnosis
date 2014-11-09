using Diagnosis.Data;
using NHibernate;

namespace Diagnosis.ViewModels
{
    public class SessionVMBase : ViewModelBase
    {
        private ISession session;
        private IStatelessSession statelessSession;

        protected IStatelessSession StatelessSession
        {
            get
            {
                if (statelessSession == null)
                    statelessSession = NHibernateHelper.OpenStatelessSession();
                return statelessSession;
            }
        }

        protected ISession Session
        {
            get
            {
                return NHibernateHelper.GetSession();
            }
        }

        public SessionVMBase()
        {
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