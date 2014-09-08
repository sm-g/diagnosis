using Diagnosis.Data;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels
{
    public class SessionVMBase : ViewModelBase
    {
        ISession session;
        private bool _disposed;
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
                if (session == null)
                    session = NHibernateHelper.OpenSession();
                return session;
            }
        }
        public SessionVMBase()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (Session != null)
                        Session.Dispose();
                    if (statelessSession != null)
                        statelessSession.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
