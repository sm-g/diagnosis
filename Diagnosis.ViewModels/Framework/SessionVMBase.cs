using Diagnosis.Data;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using Diagnosis.Models;

namespace Diagnosis.ViewModels
{
    public class SessionVMBase : ViewModelBase
    {
        static NHibernateHelper nhib;
        static SessionVMBase()
        {
            if (IsInDesignMode)
            {
                Nhib.InMemory = true;
            }
        }

        public SessionVMBase()
        {
            if (IsInDesignMode && AuthorityController.CurrentDoctor == null)
            {
                var doc = Nhib.GetSession().Query<Doctor>().FirstOrDefault();
                AuthorityController.TryLogIn(doc);
            }
        }

        protected ISession Session
        {
            get
            {
                return Nhib.GetSession();
            }
        }

        public static NHibernateHelper Nhib
        {
            get
            {
                if (nhib == null) nhib = NHibernateHelper.Default;
                return nhib;
            }
            set
            {
                nhib = value;
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