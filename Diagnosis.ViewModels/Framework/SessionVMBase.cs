using Diagnosis.Data;
using NHibernate;

namespace Diagnosis.ViewModels
{
    public class SessionVMBase : ViewModelBase
    {
        static SessionVMBase()
        {
            if (IsInDesignMode)
            {
                NHibernateHelper.Default.InMemory = true;
            }
        }

        public SessionVMBase()
        {
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
            }

            base.Dispose(disposing);
        }
    }
}