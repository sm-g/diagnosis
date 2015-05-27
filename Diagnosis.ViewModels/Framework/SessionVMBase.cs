using Diagnosis.Data;
using NHibernate;
using EventAggregator;
using Diagnosis.Common;

namespace Diagnosis.ViewModels
{
    public class SessionVMBase : ViewModelBase
    {
        static NHibernateHelper nhib;
        ISession _session;
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
            _session = Nhib.GetSession();
            handler = this.Subscribe(Event.NewSession, (e) =>
            {
                var s = e.GetValue<ISession>(MessageKeys.Session);
                if (_session.SessionFactory == s.SessionFactory)
                    _session = s;

            });
        }

        protected ISession Session
        {
            get
            {
                return _session;
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
                handler.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}