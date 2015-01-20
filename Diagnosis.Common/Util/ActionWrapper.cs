using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Common.Util
{
    public class FlagActionWrapper<T>
    {
        private bool flag = false;

        private T list;

        private Action<T> act;

        public FlagActionWrapper(Action<T> act)
        {
            Contract.Requires(act != null);

            this.act = act;
        }

        public bool CanEnter
        {
            get { return !flag; }
        }

        public ActionWrapperHandler Enter(T list)
        {
            Contract.Requires(list != null);

            if (flag)
                throw new InvalidOperationException();

            this.list = list;
            return new ActionWrapperHandler(this);
        }

        public class ActionWrapperHandler : IDisposable
        {
            private FlagActionWrapper<T> owner;

            public ActionWrapperHandler(FlagActionWrapper<T> owner)
            {
                this.owner = owner;
                owner.flag = true;
            }

            public void Dispose()
            {
                owner.act(owner.list);
                owner.flag = false;
            }
        }
    }
}