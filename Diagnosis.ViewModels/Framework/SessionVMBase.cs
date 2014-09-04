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
        protected ISession session;
        private bool _disposed;

        public SessionVMBase()
        {
            session = NHibernateHelper.OpenSession();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    session.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
