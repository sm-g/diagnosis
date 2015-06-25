using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using EventAggregator;
using NHibernate;
using NHibernate.Linq;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class SessionVMBase : ViewModelBase
    {
        private static NHibernateHelper nhib;
        private ISession _session;
        private EventMessageHandler handler;

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
            _session = Nhib.GetSession();
            handler = this.Subscribe(Event.NewSession, (e) =>
            {
                var s = e.GetValue<ISession>(MessageKeys.Session);
                ReplaceSession(s);
            });
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

        protected ISession Session
        {
            get
            {
                return _session;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                handler.Dispose();
            }
            base.Dispose(disposing);
        }

        private void ReplaceSession(ISession s)
        {
            if (_session.SessionFactory == s.SessionFactory)
                _session = s;
        }
    }
}