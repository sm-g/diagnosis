using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Common.Util
{
    public class ListActionWrapper<T>
    {
        private bool flag = false;

        private List<T> list;

        private Action<T> act;

        public ListActionWrapper(IEnumerable<T> list, Action<T> act)
        {
            Contract.Requires(list != null);
            Contract.Requires(act != null);

            this.list = list.ToList();
            this.act = act;
        }

        public ListActionWrapper(Action<T> act)
        {
            Contract.Requires(act != null);

            this.act = act;
        }

        public bool CanEnter
        {
            get { return !flag; }
        }

        public ActionWrapperHandler Enter()
        {
            if (flag)
                throw new InvalidOperationException();

            return new ActionWrapperHandler(this);
        }

        public ActionWrapperHandler Enter(IEnumerable<T> list)
        {
            Contract.Requires(list != null);

            if (flag)
                throw new InvalidOperationException();

            this.list = list.ToList();
            return new ActionWrapperHandler(this);
        }

        public class ActionWrapperHandler : IDisposable
        {
            private ListActionWrapper<T> owner;

            public ActionWrapperHandler(ListActionWrapper<T> owner)
            {
                this.owner = owner;
                owner.flag = true;
            }

            public void Dispose()
            {
                owner.list.ForEach(owner.act);
                owner.flag = false;
            }
        }
    }
}